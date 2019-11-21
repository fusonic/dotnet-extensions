using System.Collections.Generic;
using CommandLine;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

namespace Fusonic.Extensions.UnitTests.Tools.PostgreSql
{
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

        public int Execute()
        {
            PostgreSqlUtil.Cleanup(ConnectionString!, Prefix!, Exclude, DryRun);
            return 0;
        }
    }
}