// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Aspire.Hosting;

public sealed class ProtectedSecretsOptions
{
    /// <summary>
    /// Identifies the AppHost's secrets in the store. Defaults to the AppHost's <c>UserSecretsId</c>, or its name if it
    /// has none.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Where the encrypted file lives on Windows (elsewhere MSAL's storage keeps a marker file there). Defaults to
    /// <c>Fusonic.Extensions.Aspire</c> in the local application data folder.
    /// </summary>
    public string? Directory { get; set; }
}
