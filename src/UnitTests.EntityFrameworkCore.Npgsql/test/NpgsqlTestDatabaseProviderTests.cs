// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;
public class NpgsqlTestDatabaseProviderTests : TestBase
{
    private static readonly List<string> UsedDbNames = new();

    public NpgsqlTestDatabaseProviderTests(TestFixture fixture) : base(fixture)
    { }

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
            return PostgreSqlUtil.GetDatabaseName(ctx.Database.GetConnectionString()!)!;
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
        var provider = GetInstance<NpgsqlTestDatabaseProvider>();
        provider.CreateDatabase();

        // Act
        provider.DropDatabase();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().NotContain(provider.TestDbName);
    }

    [Fact]
    public async Task DropDatabase_DatabaseDoesNotExist_DoesNotThrowException()
    {
        // Arrange
        var provider = GetInstance<NpgsqlTestDatabaseProvider>();

        // Act
        provider.DropDatabase();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().NotContain(provider.TestDbName);
    }

    [Fact]
    public async Task CreateDatabase_CreatesTestDb()
    {
        // Arrange
        var provider = GetInstance<NpgsqlTestDatabaseProvider>();

        // Act
        provider.CreateDatabase();

        // Assert
        var dbNames = await GetDatabases();
        dbNames.Should().Contain(provider.TestDbName);
    }

    [Fact]
    public void TestDbName_IsDifferentThanTemplate()
    {
        var provider = GetInstance<NpgsqlTestDatabaseProvider>();
        var settings = GetInstance<NpgsqlTestDatabaseProviderSettings>();
        var dbName = new NpgsqlConnectionStringBuilder(settings.TemplateConnectionString).Database;

        provider.TestDbName.Should().NotBe(dbName);
    }

    private async Task<List<string>> GetDatabases()
    {
        var settings = GetInstance<NpgsqlTestDatabaseProviderSettings>();
        var builder = new NpgsqlConnectionStringBuilder(settings.TemplateConnectionString);
        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        var dbNames = (await connection.QueryAsync<string>("SELECT datname FROM pg_database")).ToList();
        return dbNames;
    }
}
