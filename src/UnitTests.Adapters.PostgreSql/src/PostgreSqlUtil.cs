using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Drops all test databases with the given prefix. Excludes those from deleting that continue with one of the strings in the exclude-parameter.
        /// Example:
        ///   Prefix = Test_
        ///   Exclude = branch1, branch2
        /// Deletes all databases beginning with Test_ except those beginning with Test_branch1 and Test_branch2.
        /// 
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbPrefix">Prefix of the test databases that should be dropped.</param>
        /// <param name="exclude">Excludes databases from deleting that continue with one of the strings in the exclude-parameter.</param>
        /// <param name="dryRun">Does not drop the databases. Only outputs the databases that would get dropped.</param>
        public static void Cleanup(string connectionString, string dbPrefix, IEnumerable<string>? exclude = null, bool dryRun = false)
        {
            EnsureNotPostgres(dbPrefix);

            var ignoreDbs = exclude?.Select(e => dbPrefix + e).ToList() ?? new List<string>();

            using var connection = CreatePostgresDbConnection(connectionString);
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
                
                if (ignoreDbs.Any(dbName.StartsWith))
                    continue;

                if (dryRun)
                    Console.Out.WriteLine($"[DryRun] Would drop {dbName}");
                
                else
                {
                    Console.Out.WriteLine($"Dropping {dbName}");
                    DropDb(connectionString, dbName);
                }
            }
        }

        /// <summary>
        /// Drops the given database. If there are still users connected to the database their sessions will be terminated.
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbName">Database that should be dropped.</param>
        public static void DropDb(string connectionString, string dbName)
        {
            EnsureNotPostgres(dbName);
            
            using var connection = CreatePostgresDbConnection(connectionString);
            connection.Open();
            connection.Execute($"ALTER DATABASE \"{dbName}\" CONNECTION LIMIT 0");
            TerminateUsers(dbName, connection);
            connection.Execute($"DROP DATABASE \"{dbName}\"");
        }

        /// <summary>
        /// Terminates all users on a database
        /// </summary>
        /// <param name="connectionString">Connection string to the postgres database.</param>
        /// <param name="dbName">Name of the database where the sessions should be terminated.</param>
        public static void TerminateUsers(string connectionString, string dbName)
        {
            using var connection = CreatePostgresDbConnection(connectionString);
            connection.Open();
            TerminateUsers(dbName, connection);
        }

        private static void TerminateUsers(string dbName, NpgsqlConnection postgresDbConnection)
        {
            postgresDbConnection.Execute($@"SELECT pg_terminate_backend(pg_stat_activity.pid)
                                            FROM pg_stat_activity
                                            WHERE pg_stat_activity.datname = '{dbName}'
                                            AND pid <> pg_backend_pid()");
        }

        /// <summary> Creates a test database. </summary>
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
            using var connection = CreatePostgresDbConnection(connectionString);
            connection.Open();

            //get database from connection string. Also safeguard against changes on admin-DB.
            var dbName = GetDatabase(connectionString);
            if (dbName == null)
                throw new ArgumentException("Could not find database name in connection string.");

            if (dbName == "postgres")
                throw new ArgumentException("I don't think the postgres database should be your test template...'");

            //Drop existing Test-DB
            if (connection.ExecuteScalar<bool>($"SELECT EXISTS(SELECT * FROM pg_catalog.pg_database WHERE datname='{dbName}')"))
            {
                Console.WriteLine("Dropping database " + dbName);
                connection.Execute($"ALTER DATABASE \"{dbName}\" IS_TEMPLATE false");
                DropDb(connectionString, dbName);
            }

            //Create database
            Console.WriteLine("Creating database " + dbName);
            connection.Execute($"CREATE DATABASE \"{dbName}\" TEMPLATE template0 IS_TEMPLATE true");

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
            connection.Execute($"ALTER DATABASE \"{dbName}\" CONNECTION LIMIT 0");
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

        /// <summary> Creates a connection using the given connection string, but replacing the database with postgres. </summary>
        private static NpgsqlConnection CreatePostgresDbConnection(string connectionString) 
            => new NpgsqlConnection(ReplaceDb(connectionString, "postgres"));

        /// <summary> Replaces the database in a connection string with another one. </summary>
        public static string ReplaceDb(string connectionString, string dbName)
            => getDatabaseRegex.Replace(connectionString, $"Database={dbName}");

        /// <summary> Returns the database name in the connection string or null, if it could not be matched. </summary>
        public static string? GetDatabase(string connectionString)
        {
            var match = getDatabaseRegex.Match(connectionString);
            if (!match.Success || match.Groups.Count != 2)
                return null;

            return match.Groups[1].Value;
        }

        private static void EnsureNotPostgres(string dbName)
        {
            if ("postgres".Equals(dbName?.ToLower().Trim()))
                throw new ArgumentException("You can't do this on the postgres database.");
        }
    }
}