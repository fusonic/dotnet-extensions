// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.EntityFrameworkCore.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.EntityFrameworkCore.Tests.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<SampleDomainEntity> SampleDomainEntities { get; set; } = null!;
}