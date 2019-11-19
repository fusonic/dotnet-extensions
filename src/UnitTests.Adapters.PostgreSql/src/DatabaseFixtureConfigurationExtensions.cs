﻿using System;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql
{
    public static class DatabaseFixtureConfigurationExtensions
    {
        /// <summary>
        /// Registers a provider to use in PostgreSQL databases in unit tests. Use the [PostgreSqlTest]-attribute on class- or method-level to use that provider.
        /// </summary>
        /// <param name="configuration">The config where the provider should be registered on.</param>
        /// <param name="connectionString">The connection string points to the postgres database. You don't know the name of the test DB in this context. The provider takes care of that.</param>
        /// <param name="dbNamePrefix">All test databases are created with the prefix (ie. `YourProjectTest`). Use the branch name or sth. like that when running in a CI pipeline.</param>
        /// <param name="templateDb">When a template is defined, the test database will initially be copied from it (ie. `YourProjectTest_Template`). When a template is set, migrations won't be executed.
        /// The template is expected to be up to date. Use <see cref="PostgreSqlUtil" /> for template creation support.</param>
        /// <param name="optionsBuilder">The options builder for Npgsql if the context requires one. Gets used in DbContextOptionsBuilder{TDbContext}().UseNpgsql(). Example: o => o.UseNodaTime().</param>
        /// <param name="seed">The seed that should be executed when this provider is used.</param>
        public static DatabaseFixtureConfiguration<TDbContext> UsePostgreSqlDatabase<TDbContext>(
            this DatabaseFixtureConfiguration<TDbContext> configuration,
            string connectionString,
            string dbNamePrefix,
            string? templateDb = null,
            Action<NpgsqlDbContextOptionsBuilder>? optionsBuilder = null,
            Func<TDbContext, Task>? seed = null)
            where TDbContext : DbContext
        {
            configuration.RegisterProvider(typeof(PostgreSqlTestAttribute),
                attr =>
                {
                    var attribute = (PostgreSqlTestAttribute)attr;
                    return new PostgreSqlDatabaseProvider<TDbContext>(connectionString, dbNamePrefix, templateDb, attribute.EnableLogging, optionsBuilder, seed);
                });
            return configuration;
        }
    }
}