// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql.Tests;

public class TestDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<TestDbContext>().UseNpgsql("Host=localhost;Database=fusonic_extensions;Username=postgres;Password=developer").Options);
}