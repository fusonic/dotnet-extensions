// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class NpgsqlDatabasePerTestStoreTests(TestFixture fixture) : TestBase(fixture)
{
    private static readonly List<string> UsedDbNames = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task EachTestGetsAnOwnDatabase_TestsDoNotAffectEachOther(int entityCount)
    {
        // Arrange
        var dbName = await QueryAsync(async ctx =>
        {
            ctx.AddRange(Enumerable.Range(1, entityCount).Select(i => new TestEntity { Name = "Name " + i }));
            await ctx.SaveChangesAsync();
            return new NpgsqlConnectionStringBuilder(ctx.Database.GetConnectionString()).Database!;
        });

        // Assert unique db names (not nice)
        UsedDbNames.Should().NotContain(dbName);
        UsedDbNames.Add(dbName);

        // Assert entities
        var count = await QueryAsync(ctx => ctx.TestEntities.CountAsync());
        count.Should().Be(entityCount);
    }

    [Fact]
    public async Task DropDatabase_DatabaseExists_DropsDatabase()
    {
        // Arrange
        var store = GetStore();
        await store.CreateDatabase();

        // Act
        await store.OnTestEnd();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().NotContain(store.ConnectionString);
    }

    [Fact]
    public async Task DropDatabase_DatabaseDoesNotExist_DoesNotThrowException()
    {
        // Arrange
        var store = GetStore();

        // Act
        await store.OnTestEnd();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().NotContain(GetDbName(store));
    }

    [Fact]
    public async Task CreateDatabase_CreatesTestDb()
    {
        // Arrange
        var store = GetStore();

        // Act
        await store.CreateDatabase();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().Contain(GetDbName(store));
    }

    [Fact]
    public void TestDbName_IsDifferentThanTemplate()
    {
        var store = GetStore();
        var settings = GetInstance<NpgsqlDatabasePerTestStoreOptions>();
        GetDbName(settings.ConnectionString!).Should().NotBe(GetDbName(store));
    }

    private async Task<List<string>> GetDatabases()
    {
        var settings = GetInstance<NpgsqlDatabasePerTestStoreOptions>();
        var builder = new NpgsqlConnectionStringBuilder(settings.ConnectionString);
        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        var dbNames = (await connection.QueryAsync<string>("SELECT datname FROM pg_database")).ToList();
        return dbNames;
    }

    private NpgsqlDatabasePerTestStore GetStore() => (NpgsqlDatabasePerTestStore)GetInstance<ITestStore>();
    private static string GetDbName(NpgsqlDatabasePerTestStore store) => GetDbName(store.ConnectionString);
    private static string GetDbName(string connectionString) => new NpgsqlConnectionStringBuilder(connectionString).Database!;
}