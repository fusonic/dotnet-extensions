// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Fusonic.Extensions.UnitTests;

public static class TestConfigurationHelper
{
    public static IConfigurationBuilder ConfigureTestDefault(this IConfigurationBuilder builder, string basePath, Assembly assembly)
        => builder.SetBasePath(basePath)
                  .AddJsonFile("testsettings.json", optional: true)
                  .AddUserSecrets(assembly, optional: true)
                  .AddEnvironmentVariables();

    public static IConfiguration GetDefaultConfiguration(string basePath, Assembly assembly)
        => new ConfigurationBuilder()
          .ConfigureTestDefault(basePath, assembly)
          .Build();
}