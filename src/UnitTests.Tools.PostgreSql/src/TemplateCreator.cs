// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql;

internal static class TemplateCreator
{
    public const string Verb = "exectemplatecreation";

    private static readonly Regex GetDatabaseRegex = new("Database=([^;]+)", RegexOptions.Compiled);

    internal static int Run(string[] args)
    {
        //As we run in the same dependency context & working directory of the targeted assembly (likely a .Test.dll of a completely different project),
        //it is not guaranteed, that we have access to any lib other than the basic System.* libs. We most likely won't have a CommandLineParser or
        //anything like that. Thus we rely on positional arguments for simplicity.
        GetOptions(args, out var assemblyPath, out var connectionString, out var database, out var creatorTypeName);
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

        Type? creatorType;
        if (!string.IsNullOrWhiteSpace(creatorTypeName))
        {
            creatorType = assembly.GetType(creatorTypeName, throwOnError: true);
        }
        else
        {
            const string interfaceName = nameof(ITestDbTemplateCreator); //nameof() get compiled to strings, so using non-system types there is allowed.
            var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterface(interfaceName) != null).ToList();

            if (types.Count == 0)
            {
                Console.Error.WriteLine("Could not find implementation of ITestDbTemplateCreator in given assembly.");
                return 1;
            }

            if (types.Count != 1)
            {
                Console.Error.WriteLine("Found multiple implementations of ITestDbTemplateCreator in given assembly. Specify --type to set the concrete implementation to use.");
                return 1;
            }

            creatorType = types[0];
        }

        if (!string.IsNullOrWhiteSpace(database))
            connectionString = GetDatabaseRegex.Replace(connectionString, $"Database={database}");

        var templateCreator = Activator.CreateInstance(creatorType!);
        var createMethod = creatorType!.GetMethod(nameof(ITestDbTemplateCreator.Create));
        if (createMethod == null)
        {
            Console.Error.WriteLine($"Could not find method Create on matched ITestDbTemplateCreator-type '{creatorType.Name}'");
            return 1;
        }
        createMethod.Invoke(templateCreator, new object[] { connectionString });

        return 0;
    }

    internal static void GetOptions(string[] args, out string assemblyPath, out string connectionString, out string? database, out string? creatorTypeName)
    {
        assemblyPath = args[1];
        connectionString = args[2];
        database = args[3] == "-" ? null : args[3];
        creatorTypeName = args[4] == "-" ? null : args[4];
    }

    internal static string[] GetArgs(TemplateOptions options) => new[]
    {
        Verb,
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        Path.GetFullPath(options.AssemblyPath ?? throw new ArgumentNullException(nameof(options.AssemblyPath))),
        options.ConnectionString ?? throw new ArgumentNullException(nameof(options.ConnectionString)),
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        options.Database ?? "-",
        options.CreatorTypeName ?? "-"
    };
}
