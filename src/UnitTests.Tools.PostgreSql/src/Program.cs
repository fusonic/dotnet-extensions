using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default
                         .ParseArguments<CleanupOptions, DropOptions, TemplateOptions>(args)
                         .MapResult(
                              (CleanupOptions o) => Cleanup(o),
                              (DropOptions o) => Drop(o),
                              (TemplateOptions o) => Template(o),
                              errors => 1);
        }

        private static int Cleanup(CleanupOptions options)
        {
            PostgreSqlUtil.Cleanup(options.ConnectionString!, options.Prefix!, options.Exclude, options.DryRun);
            return 0;
        }

        private static int Drop(DropOptions options)
        {
            string? dbName;
            if (!string.IsNullOrWhiteSpace(options.Database))
                dbName = options.Database;

            else
            {
                dbName = PostgreSqlUtil.GetDatabase(options.ConnectionString!);
                if (dbName == null)
                {
                    Console.Error.WriteLine("Could not find database name in connection string.");
                    return 1;
                }
            }

            PostgreSqlUtil.DropDb(options.ConnectionString!, dbName!);
            return 0;
        }

        private static int Template(TemplateOptions templateOptions)
        {
            var loadContext = new LoadContext(templateOptions.AssemblyPath!);
            var assembly = loadContext.EntryAssembly;

            Type? creatorType;
            if (!string.IsNullOrWhiteSpace(templateOptions.CreatorTypeName))
            {
                creatorType = assembly.GetType(templateOptions.CreatorTypeName, throwOnError: true);
            }
            else
            {
                var interfaceName = typeof(ITestDbTemplateCreator).FullName;
                var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterface(interfaceName) != null).ToList();

                if (types.Count == 0)
                {
                    Console.Error.WriteLine($"Could not find implementation of {nameof(ITestDbTemplateCreator)} in given assembly.");
                    return 1;
                }

                if (types.Count != 1)
                {
                    Console.Error.WriteLine($"Found multiple implementations of {nameof(ITestDbTemplateCreator)} in given assembly. Specify --type to set the concrete implementation to use.");
                    return 1;
                }

                creatorType = types[0];
            }

            var connectionString = templateOptions.ConnectionString!;
            if (!string.IsNullOrWhiteSpace(templateOptions.Database))
                connectionString = PostgreSqlUtil.ReplaceDb(connectionString, templateOptions.Database!);

            var templateCreator = Activator.CreateInstance(creatorType);
            var createMethod = creatorType.GetMethod(nameof(ITestDbTemplateCreator.Create));
            if (createMethod == null)
            {
                Console.Error.WriteLine($"Could not find method {nameof(ITestDbTemplateCreator.Create)} on matched {nameof(ITestDbTemplateCreator)}-type '{creatorType.Name}'");
                return 1;
            }
            createMethod.Invoke(templateCreator, new object[] { connectionString });

            return 0;
        }

        [Verb("cleanup", HelpText = "Deletes test databases.")]
        public class CleanupOptions
        {
            [Option('c', "connectionstring", Required = true, HelpText = "Connection string to the database.")]
            public string? ConnectionString { get; set; }

            [Option('p', "prefix", Required = true, HelpText = "All databases starting with this prefix are deleted.")]
            public string? Prefix { get; set; }

            [Option('e', "exclude", HelpText = "Exclude databases that start with the given prefix and have one of the excludes afterwards.")]
            public IEnumerable<string>? Exclude { get; set; }

            [Option(HelpText = "Do not drop databases. Only output which databases would be dropped.")]
            public bool DryRun { get; set; }
        }

        [Verb("drop", HelpText = "Drops a specific database. All connected users will be terminated before.")]
        public class DropOptions
        {
            [Option('c', "connectionstring", Required = true, HelpText = "Connection string to the database that should be dropped. Terminates all connected sessions, so dropping should always succeed (given the proper permissions).")]
            public string? ConnectionString { get; set; }

            [Option('d', "database", HelpText = "Drop a different database than the DB given in the connection string.")]
            public string? Database { get; set; }
        }

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
        }
    }
}