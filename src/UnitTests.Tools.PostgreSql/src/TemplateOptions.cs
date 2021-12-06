// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using CommandLine;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql;

[Verb("template", HelpText = "Create a test template database.")]
public class TemplateOptions
{
    [Option('c', "connectionstring", Required = true, HelpText = "Connection string to the template database. The database does not have to exist yet.")]
    public string? ConnectionString { get; set; }

    [Option('d', "database", HelpText = "Use a different database name for the template than the DB given in the connection string.")]
    public string? Database { get; set; }

    [Option('a', "assembly", Required = true, HelpText = "Path to the assembly that has an implementation of ITestDbTemplateCreator.")]
    public string? AssemblyPath { get; set; }

    [Option('t', "type", HelpText = "Concrete type (FullName) of the implementation of ITestDbTemplateCreator. Only required, if an assembly has multiple implementations of this interface.")]
    public string? CreatorTypeName { get; set; }

    public int Execute()
    {
        //add deps and runtime config to params
        var targetPath = Path.GetFullPath(AssemblyPath!);
        var targetDirectory = Path.GetDirectoryName(targetPath)!;
        var assemblyName = Path.GetFileNameWithoutExtension(targetPath);
        var depsFile = Path.Combine(targetDirectory, assemblyName + ".deps.json");
        var runtimeConfig = Path.Combine(targetDirectory, assemblyName + ".runtimeconfig.json");

        var args = new List<string> { "exec", "--depsfile", depsFile };
        if (File.Exists(runtimeConfig))
        {
            args.Add("--runtimeconfig");
            args.Add(runtimeConfig);
        }

        //we want to run run PostgreSql.Exec
        var currentAssembly = Path.GetFullPath(Assembly.GetExecutingAssembly().Location)!;
        args.Add(currentAssembly);

        //pass the arguments
        args.AddRange(TemplateCreator.GetArgs(this));

        return Exe.Run("dotnet", args, targetDirectory);
    }
}
