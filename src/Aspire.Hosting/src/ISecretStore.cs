// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Aspire.Hosting;

/// <summary>Where the protected secrets are kept, as one blob.</summary>
internal interface ISecretStore
{
    /// <exception cref="SecretStoreException">The store isn't available.</exception>
    byte[] Read();

    /// <exception cref="SecretStoreException">The store isn't available.</exception>
    void Write(byte[] data);
}

internal sealed class SecretStoreException(string message, Exception? innerException = null) : Exception(message, innerException);
