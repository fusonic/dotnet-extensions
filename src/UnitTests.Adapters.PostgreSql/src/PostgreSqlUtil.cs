using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Fusonic.Extensions.UnitTests.Adapters.PostgreSql
{
    /// <summary>
    /// Utilities for Postgres tests. This class is can be used from external sources, like LinqPad or PowerShell scripts
    /// </summary>
    public static class PostgreSqlUtil
    {
        private static readonly Regex getDatabaseRegex = new Regex("Database=([^;]+)");

        /// <summary>
        /// Drops all test databases with the given prefix. As the test databases normally have a pipe "|" between the prefix and the unique identifier, that pipe will be added to the given prefix
        /// as a small safeguard that only test databases get deleted.
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbPrefix">Prefix of the test databases that should be dropped.</param>
        public static void Cleanup(string connectionString, string dbPrefix)
        {
            using var connection = CreateAdminConnection(connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();

            //ensure that only test-dbs get dropped by appending the pipe used in the db names
            if (!dbPrefix.EndsWith("|"))
                dbPrefix += "|";

            cmd.CommandText = $"SELECT datname FROM pg_database WHERE datname LIKE '{dbPrefix}%'";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var dbName = (string)reader[0];
                Console.Out.WriteLine($"Dropping {dbName}");
                DropDb(connectionString, dbName);
            }
        }

        /// <summary>
        /// Drops the given database. If there are still users connected to the database their sessions will be terminated.
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbName">Database that should be dropped.</param>
        public static void DropDb(string connectionString, string dbName)
        {
            TerminateUsers(connectionString, dbName);

            using var connection = CreateAdminConnection(connectionString);
            connection.Open();
            connection.Execute($"DROP DATABASE \"{dbName}\"");
        }

        /// <summary>
        /// Terminates all users on a database
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbName">Name of the database where the sessions should be terminated.</param>
        public static void TerminateUsers(string connectionString, string dbName)
        {
            using var connection = CreateAdminConnection(connectionString);
            connection.Open();

            connection.Execute($@"SELECT pg_terminate_backend(pg_stat_activity.pid)
                                  FROM pg_stat_activity
                                  WHERE pg_stat_activity.datname = '{dbName}'
                                  AND pid <> pg_backend_pid()");
        }

        /// <summary>
        /// Creates a test database.
        /// </summary>
        /// <param name="connectionString">Connection string to the test database. The database does not have to exist.</param>
        /// <param name="dbContextFactory">Returns a DbContext using the given options.</param>
        /// <param name="npgsqlOptionsAction">The configuration action for .UseNpgsql().</param>
        /// <param name="seed">Optional seed action that gets executed after creating the database.</param>
        public static void CreateTestDbTemplate<TDbContext>(
            string connectionString,
            Func<DbContextOptions<TDbContext>, TDbContext> dbContextFactory,
            Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
            Func<TDbContext, Task>? seed = null)
            where TDbContext : DbContext
        {
            //Clear connection pools when coming from LinqPad. Otherwise consecutive calls may cause exceptions in Migrate() as it tries to reuse a terminated connection.
            NpgsqlConnection.ClearAllPools();

            //Open connection to the postgres-DB (for drop, create, alter)
            using var adminConnection = CreateAdminConnection(connectionString);
            adminConnection.Open();

            //get database from connection string. Also safeguard against changes on admin-DB.
            var dbName = GetDatabase(connectionString);
            if (dbName == null)
                throw new ArgumentException("Could not find database name in connection string.");

            if (dbName == "postgres")
                throw new ArgumentException("I don't think the postgres database should be your test template...'");

            //Drop existing Test-DB
            if (adminConnection.ExecuteScalar<bool>($"SELECT EXISTS(SELECT * FROM pg_catalog.pg_database WHERE datname='{dbName}')"))
            {
                Console.WriteLine("Dropping database " + dbName);
                adminConnection.Execute($"ALTER DATABASE \"{dbName}\" IS_TEMPLATE false");
                DropDb(connectionString, dbName);
            }

            //Create database
            Console.WriteLine("Creating database " + dbName);
            adminConnection.Execute($"CREATE DATABASE \"{dbName}\" TEMPLATE template0 IS_TEMPLATE true");

            //Migrate & run seed
            var options = new DbContextOptionsBuilder<TDbContext>().UseNpgsql(connectionString, npgsqlOptionsAction).Options;
            using (var dbContext = dbContextFactory(options))
            {
                Console.WriteLine("Running migrations");
                dbContext.Database.Migrate();

                if (seed != null)
                {
                    Console.WriteLine("Running seed");
                    seed(dbContext).Wait();
                }
            }

            //Convert to template
            Console.WriteLine("Setting connection limit on template");
            adminConnection.Execute($"ALTER DATABASE \"{dbName}\" CONNECTION LIMIT 0");
            TerminateUsers(connectionString, dbName);

            Console.WriteLine("Done");
        }

        private static void Execute(this NpgsqlConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static T ExecuteScalar<T>(this NpgsqlConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            return (T)cmd.ExecuteScalar();
        }

        private static NpgsqlConnection CreateAdminConnection(string connectionString) 
            => new NpgsqlConnection(ReplaceDb(connectionString, "postgres"));

        /// <summary>
        /// Replaces the database in a connection string with another one.
        /// </summary>
        public static string ReplaceDb(string connectionString, string dbName)
            => getDatabaseRegex.Replace(connectionString, $"Database={dbName}");

        /// <summary>
        /// Returns the database name in the connection string or null, if it could not be matched.
        /// </summary>
        public static string? GetDatabase(string connectionString)
        {
            var match = getDatabaseRegex.Match(connectionString);
            if (!match.Success || match.Groups.Count != 2)
                return null;

            return match.Groups[1].Value;
        }
    }
}