// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Identity.Client.Extensions.Msal;

namespace Fusonic.Extensions.Aspire.Hosting;

/// <summary>
/// MSAL's cache storage, used for any data: the maintained cross-platform wrapper around DPAPI (Windows), the Keychain
/// (macOS) and the Secret Service (Linux, libsecret).
/// </summary>
internal sealed class MsalSecretStore(string name, string directory) : ISecretStore
{
    private const string Product = "Fusonic.Extensions.Aspire";

    private readonly Lazy<Storage> storage = new(() => Storage.Create(
        new StorageCreationPropertiesBuilder($"{name}.secrets", directory)
           .WithMacKeyChain(Product, name)
           .WithLinuxKeyring(
                $"{Product}.secrets",
                MsalCacheHelper.LinuxKeyRingDefaultCollection,
                $"Aspire AppHost secrets ({name})",
                new KeyValuePair<string, string>("Version", "1"),
                new KeyValuePair<string, string>("AppHost", name))
           .Build()));

    public byte[] Read() => Run(() => storage.Value.ReadData());

    public void Write(byte[] data) => Run(() =>
    {
        storage.Value.WriteData(data);
        return data;
    });

    private static T Run<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (MsalCachePersistenceException exception)
        {
            throw new SecretStoreException(exception.Message, exception);
        }
    }
}
