// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

#pragma warning disable ASPIREUSERSECRETS001

namespace Fusonic.Extensions.Aspire.Hosting.Tests;

public class ProtectedSecretsTests
{
    [Fact]
    public void UseProtectedSecrets_ReplacesUserSecretsManager()
    {
        var builder = DistributedApplication.CreateBuilder();

        builder.UseProtectedSecrets(new FakeSecretStore());

        using var services = builder.Services.BuildServiceProvider();
        services.GetRequiredService<IUserSecretsManager>().Should().BeOfType<ProtectedUserSecretsManager>();
    }

    [Fact]
    public void UseProtectedSecrets_LoadsStoredSecrets()
    {
        var builder = DistributedApplication.CreateBuilder();
        var store = new FakeSecretStore(new() { ["Parameters:password"] = "stored" });

        builder.UseProtectedSecrets(store);

        builder.Configuration["Parameters:password"].Should().Be("stored");
    }

    [Fact]
    public void UseProtectedSecrets_StoreUnavailable_KeepsAspiresManager()
    {
        var builder = DistributedApplication.CreateBuilder();
        var original = builder.Services.Last(s => s.ServiceType == typeof(IUserSecretsManager));

        builder.UseProtectedSecrets(new FakeSecretStore { Available = false });

        builder.Services.Last(s => s.ServiceType == typeof(IUserSecretsManager)).Should().BeSameAs(original);
    }

    [Fact]
    public void AddAfterUserSecrets_InsertsRightAfterUserSecrets()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["key"] = "appsettings" });
        configuration.Sources.Add(new JsonConfigurationSource { Path = "secrets.json", Optional = true });
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["key"] = "environment" });

        ProtectedSecretsExtensions.AddAfterUserSecrets(configuration, new() { ["key"] = "stored", ["other"] = "stored" });

        var userSecrets = configuration.Sources.ToList().FindIndex(s => s is JsonConfigurationSource { Path: "secrets.json" });
        configuration.Sources[userSecrets + 1].Should().BeOfType<MemoryConfigurationSource>()
           .Which.InitialData.Should().Contain(KeyValuePair.Create("other", (string?)"stored"));
        configuration["key"].Should().Be("environment");
        configuration["other"].Should().Be("stored");
    }

    [Fact]
    public void AddAfterUserSecrets_NoUserSecrets_AddsLast()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["key"] = "appsettings" });

        ProtectedSecretsExtensions.AddAfterUserSecrets(configuration, new() { ["key"] = "stored" });

        configuration["key"].Should().Be("stored");
    }

    [Fact]
    public void TrySetSecret_WritesToStore_NotToAspiresManager()
    {
        var (manager, store, inner) = CreateManager();

        manager.TrySetSecret("Parameters:a", "1").Should().BeTrue();
        manager.TrySetSecret("Parameters:b", "2").Should().BeTrue();

        store.Secrets.Should().BeEquivalentTo(new Dictionary<string, string> { ["Parameters:a"] = "1", ["Parameters:b"] = "2" });
        inner.Calls.Should().BeEmpty();
    }

    [Fact]
    public void TryDeleteSecret_RemovesFromStore()
    {
        var (manager, store, _) = CreateManager(new() { ["Parameters:a"] = "1", ["Parameters:b"] = "2" });

        manager.TryDeleteSecret("Parameters:a").Should().BeTrue();

        store.Secrets.Should().BeEquivalentTo(new Dictionary<string, string> { ["Parameters:b"] = "2" });
    }

    [Fact]
    public void TrySetSecret_StoreUnavailable_ReturnsFalse()
    {
        var (manager, store, _) = CreateManager();
        store.Available = false;

        manager.TrySetSecret("Parameters:a", "1").Should().BeFalse();
    }

    [Fact]
    public void GetOrSetSecret_Missing_GeneratesStoresAndSetsConfiguration()
    {
        var (manager, store, _) = CreateManager();
        var configuration = new ConfigurationManager();

        manager.GetOrSetSecret(configuration, "Parameters:password", () => "generated");

        configuration["Parameters:password"].Should().Be("generated");
        store.Secrets.Should().ContainKey("Parameters:password").WhoseValue.Should().Be("generated");
    }

    [Fact]
    public void GetOrSetSecret_Existing_KeepsValue()
    {
        var (manager, store, _) = CreateManager();
        var configuration = new ConfigurationManager { ["Parameters:password"] = "existing" };

        manager.GetOrSetSecret(configuration, "Parameters:password", () => throw new InvalidOperationException("Must not generate."));

        configuration["Parameters:password"].Should().Be("existing");
        store.Secrets.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveStateAsync_StaysWithAspiresManager()
    {
        var (manager, store, inner) = CreateManager();

        await manager.SaveStateAsync(new JsonObject { ["deployment"] = "state" }, TestContext.Current.CancellationToken);

        inner.Calls.Should().Equal(nameof(IUserSecretsManager.SaveStateAsync));
        store.Secrets.Should().BeEmpty();
    }

    [Fact]
    public void FilePath_IsAspires_IsAvailable_True()
    {
        var (manager, _, _) = CreateManager();

        manager.FilePath.Should().Be("inner/secrets.json");
        manager.IsAvailable.Should().BeTrue();
    }

    private static (ProtectedUserSecretsManager Manager, FakeSecretStore Store, FakeUserSecretsManager Inner) CreateManager(
        Dictionary<string, string>? secrets = null)
    {
        var store = new FakeSecretStore(secrets);
        var inner = new FakeUserSecretsManager();
        return (new ProtectedUserSecretsManager(inner, new ProtectedSecretStore(store), NullLogger.Instance), store, inner);
    }

    private sealed class FakeSecretStore(Dictionary<string, string>? secrets = null) : ISecretStore
    {
        private byte[] data = secrets is null ? [] : JsonSerializer.SerializeToUtf8Bytes(secrets);

        public bool Available { get; set; } = true;

        public Dictionary<string, string> Secrets =>
            data.Length == 0 ? [] : JsonSerializer.Deserialize<Dictionary<string, string>>(data)!;

        public byte[] Read() => Available ? data : throw new SecretStoreException("Not available.");

        public void Write(byte[] data)
        {
            if (!Available)
                throw new SecretStoreException("Not available.");

            this.data = data;
        }
    }

    private sealed class FakeUserSecretsManager : IUserSecretsManager
    {
        public List<string> Calls { get; } = [];

        public bool IsAvailable => true;

        public string FilePath => "inner/secrets.json";

        public bool TrySetSecret(string name, string value)
        {
            Calls.Add(nameof(TrySetSecret));
            return true;
        }

        public bool TryDeleteSecret(string name)
        {
            Calls.Add(nameof(TryDeleteSecret));
            return true;
        }

        public void GetOrSetSecret(IConfigurationManager configuration, string name, Func<string> valueGenerator) =>
            Calls.Add(nameof(GetOrSetSecret));

        public Task SaveStateAsync(JsonObject state, CancellationToken cancellationToken = default)
        {
            Calls.Add(nameof(SaveStateAsync));
            return Task.CompletedTask;
        }
    }
}
