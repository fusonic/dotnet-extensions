// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;

public class DatabaseUnitTestTests : TestBase
{
    public DatabaseUnitTestTests(TestFixture fixture) : base(fixture)
    { }

    [Fact]
    public void Query_ActsAndReturnsResult()
    {
        Query(ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Name = "Hi" });
            ctx.SaveChanges();
        });

        var count = Query(ctx => ctx.TestEntities.Count());
        count.Should().Be(1);
    }

    [Fact]
    public void Query_NoSave_DoesNotAffectDbContextInDifferentScope()
    {
        Query(ctx => ctx.TestEntities.Add(new TestEntity { Name = "Hi" }));

        var count = Query(ctx => ctx.TestEntities.Count());
        count.Should().Be(0);
    }

    [Fact]
    public async Task Query_WithAsyncAction_ThrowsException()
    {
        var act = () => Query(async ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Name = "Hi" });
            await ctx.SaveChangesAsync();
        });

        (await act.Should().ThrowAsync<InvalidOperationException>())
           .Which.Message.Should()
           .Be("This is the wrong method for async queries. Use QueryAsync() instead.");
    }

    [Fact]
    public async Task QueryAsync_ActsAndReturnsResult()
    {
        await QueryAsync(async ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Name = "Hi" });
            await ctx.SaveChangesAsync();
        });

        var count = Query(ctx => ctx.TestEntities.Count());
        count.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_NoSave_DoesNotAffectDbContextInDifferentScope()
    {
        await QueryAsync(ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Name = "Hi" });
            return Task.CompletedTask;
        });

        var count = await QueryAsync(ctx => ctx.TestEntities.CountAsync());
        count.Should().Be(0);
    }
}