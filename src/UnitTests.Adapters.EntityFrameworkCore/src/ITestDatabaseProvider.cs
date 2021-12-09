// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

public interface ITestDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    /// <summary> Returns the options of the DbContext pointing to a usable database. </summary>
    DbContextOptions<TDbContext> GetContextOptions();

    /// <summary> Seeds the database. </summary>
    void SeedDatabase(TDbContext dbContext);

    /// <summary> Drops the database that was created. </summary>
    void DropDatabase(TDbContext dbContext);

    /// <summary>
    /// Creates the database if it does not exist yet.
    /// You usually only need to call this if you have DB access without using EntityFramework.
    /// The PostgreSql database provider initializes the DB on the first access when using EntityFramework.
    /// </summary>
    void CreateDatabase(TDbContext dbContext);
}
