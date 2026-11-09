// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text;

namespace Fusonic.Extensions.Aspire.Hosting;

internal static class ComposeNames
{
    private const string AppHostSuffix = ".AppHost";

    public static string DefaultProjectName(string applicationName) =>
        applicationName.EndsWith(AppHostSuffix, StringComparison.OrdinalIgnoreCase) && applicationName.Length > AppHostSuffix.Length
            ? applicationName[..^AppHostSuffix.Length]
            : applicationName;

    /// <summary>
    /// Compose allows lowercase letters, digits, <c>-</c> and <c>_</c>, starting with a letter or digit. Anything else
    /// becomes <c>-</c>.
    /// </summary>
    public static string Normalize(string name)
    {
        var result = new StringBuilder(name.Length);
        foreach (var c in name.ToLowerInvariant())
        {
            result.Append(c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_' or '-' ? c : '-');
        }

        return result.ToString().Trim('-', '_');
    }
}
