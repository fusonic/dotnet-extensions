// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class TestDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<TestDbContext>().UseSqlite("Filename=:memory:").Options);
}