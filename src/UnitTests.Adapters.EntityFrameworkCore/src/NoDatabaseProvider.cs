// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

public class NoDatabaseProvider<TDbContext> : ITestDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    public DbContextOptions<TDbContext> GetContextOptions()
        => new DbContextOptions<TDbContext>();

    public void SeedDb(TDbContext dbContext)
    { }

    public void DropDb(TDbContext dbContext)
    { }
}
