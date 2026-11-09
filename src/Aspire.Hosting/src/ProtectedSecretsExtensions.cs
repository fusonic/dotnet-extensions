// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Aspire.Hosting.ApplicationModel;
using Fusonic.Extensions.Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// IUserSecretsManager is experimental, but the only place where the dashboard's "save" can be redirected; if it changes,
// this breaks at build time, not silently.
#pragma warning disable ASPIREUSERSECRETS001

// Aspire's convention for AppHost extensions: available in the AppHost without a using.
#pragma warning disable IDE0130
namespace Aspire.Hosting;
#pragma warning restore IDE0130

/// <summary>
/// Keeps parameters entered in the dashboard in the OS's store for the current user instead of in the plaintext user
/// secrets: DPAPI on Windows, the Keychain on macOS, the Secret Service (libsecret, e.g. GNOME Keyring) on Linux.
/// </summary>
/// <remarks>
/// Generated parameter defaults still go to the user secrets: Aspire saves them through
/// <see cref="IDistributedApplicationBuilder.UserSecretsManager"/>, which is created by the builder, not resolved from DI.
/// </remarks>
public static class ProtectedSecretsExtensions
{
    private const string LoggerCategory = "Fusonic.Extensions.Aspire.Hosting.ProtectedSecrets";

    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Loads the stored secrets over the user secrets (a stale value there doesn't win; environment variables and
        /// command line arguments still do) and saves new ones to the store. Where no store is available (Linux without
        /// a keyring, e.g. headless or WSL), Aspire's plaintext user secrets stay in use.
        /// </summary>
        public IDistributedApplicationBuilder UseProtectedSecrets(Action<ProtectedSecretsOptions>? configure = null)
        {
            var options = new ProtectedSecretsOptions();
            configure?.Invoke(options);

            var name = SanitizeFileName(options.Name ?? DefaultName(builder));
            var directory = options.Directory
                            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fusonic.Extensions.Aspire");

            return builder.UseProtectedSecrets(new MsalSecretStore(name, directory));
        }
    }

    internal static IDistributedApplicationBuilder UseProtectedSecrets(this IDistributedApplicationBuilder builder, ISecretStore secretStore)
    {
        var store = new ProtectedSecretStore(secretStore);

        Dictionary<string, string> secrets;
        try
        {
            secrets = store.Read();
        }
        catch (SecretStoreException exception)
        {
            // No logger yet; logged once the application starts.
            builder.Eventing.Subscribe<BeforeStartEvent>((@event, _) =>
            {
                @event.Services.GetRequiredService<ILoggerFactory>()
                   .CreateLogger(LoggerCategory)
                   .LogWarning("No OS secret store available, secrets stay in the plaintext user secrets: {Reason}", exception.Message);
                return Task.CompletedTask;
            });
            return builder;
        }

        AddAfterUserSecrets(builder.Configuration, secrets);

        var original = builder.Services.Last(service => service.ServiceType == typeof(IUserSecretsManager));
        builder.Services.Remove(original);
        builder.Services.AddSingleton<IUserSecretsManager>(services => new ProtectedUserSecretsManager(
            Resolve(original, services),
            store,
            services.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory)));

        return builder;
    }

    /// <summary>Right after the user secrets, so environment variables and command line arguments still win.</summary>
    internal static void AddAfterUserSecrets(IConfigurationBuilder configuration, Dictionary<string, string> secrets)
    {
        var source = new MemoryConfigurationSource { InitialData = secrets.Select(secret => KeyValuePair.Create(secret.Key, (string?)secret.Value)) };

        var index = configuration.Sources.Count - 1;
        while (index >= 0 && configuration.Sources[index] is not JsonConfigurationSource { Path: "secrets.json" })
        {
            index--;
        }

        if (index < 0)
            configuration.Sources.Add(source);
        else
            configuration.Sources.Insert(index + 1, source);
    }

    private static string DefaultName(IDistributedApplicationBuilder builder) =>
        (builder.AppHostAssembly ?? Assembly.GetEntryAssembly())?.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId
        ?? builder.Environment.ApplicationName;

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }

    private static IUserSecretsManager Resolve(ServiceDescriptor descriptor, IServiceProvider services) =>
        (IUserSecretsManager)(descriptor.ImplementationInstance
                              ?? descriptor.ImplementationFactory?.Invoke(services)
                              ?? ActivatorUtilities.CreateInstance(services, descriptor.ImplementationType!));
}
