// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;

namespace Fusonic.Extensions.Aspire.Hosting;

/// <summary>The secrets as name/value pairs, kept as JSON in an <see cref="ISecretStore"/>.</summary>
internal sealed class ProtectedSecretStore(ISecretStore store)
{
    private readonly Lock writeLock = new();

    /// <exception cref="SecretStoreException">The store isn't available.</exception>
    public Dictionary<string, string> Read() =>
        store.Read() is { Length: > 0 } json ? JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [] : [];

    /// <exception cref="SecretStoreException">The store isn't available.</exception>
    public void Change(Action<Dictionary<string, string>> change)
    {
        lock (writeLock)
        {
            var secrets = Read();
            change(secrets);
            store.Write(JsonSerializer.SerializeToUtf8Bytes(secrets));
        }
    }
}
