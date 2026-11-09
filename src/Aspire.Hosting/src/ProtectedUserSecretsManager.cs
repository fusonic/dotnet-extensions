// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json.Nodes;
using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable ASPIREUSERSECRETS001

namespace Fusonic.Extensions.Aspire.Hosting;

/// <summary>
/// Secrets go to the protected store; the deployment state (<c>aspire publish</c>/<c>deploy</c>) stays with Aspire's own
/// manager.
/// </summary>
internal sealed class ProtectedUserSecretsManager(IUserSecretsManager inner, ProtectedSecretStore store, ILogger logger) : IUserSecretsManager
{
    public bool IsAvailable => true;

    public string FilePath => inner.FilePath;

    public bool TrySetSecret(string name, string value) => TryChange(secrets => secrets[name] = value);

    public bool TryDeleteSecret(string name) => TryChange(secrets => secrets.Remove(name));

    public void GetOrSetSecret(IConfigurationManager configuration, string name, Func<string> valueGenerator)
    {
        if (configuration[name] is not null)
            return;

        var value = valueGenerator();
        TrySetSecret(name, value);
        configuration[name] = value;
    }

    public Task SaveStateAsync(JsonObject state, CancellationToken cancellationToken = default) => inner.SaveStateAsync(state, cancellationToken);

    private bool TryChange(Action<Dictionary<string, string>> change)
    {
        try
        {
            store.Change(change);
            return true;
        }
        catch (SecretStoreException exception)
        {
            logger.LogWarning(exception, "Couldn't save to the OS's secret store.");
            return false;
        }
    }
}
