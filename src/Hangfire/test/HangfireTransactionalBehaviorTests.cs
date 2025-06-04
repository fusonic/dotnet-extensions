// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;
using Dapper;
using Fusonic.Extensions.Common.Transactions;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.EntityFrameworkCore.Npgsql;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Fusonic.Extensions.Hangfire.Tests;

// "Do not test external libraries, they should be already tested by themselves." does not count here as Hangfire has introduced
// bugs in core usage scenarios multiple times, mainly transaction handling. To avoid surprises when updating hangfire, we test those scenarios here.
public class HangfireTransactionalBehaviorTests(HangfireTransactionalBehaviorTests.HangfireFixture fixture) : TestBase<HangfireTransactionalBehaviorTests.HangfireFixture>(fixture)
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Explicitly create DB for tests not using EF
        await GetTestStore().CreateDatabase();
    }

    [Fact]
    public void PostgreSqlStorageOptions_EnableTransactionScopeEnlistment_Default_ShouldBeTrue()
        => new PostgreSqlStorageOptions().EnableTransactionScopeEnlistment.Should().BeTrue();

    [Fact]
    public async Task CanEnqueueJob_WithinTransactionScope()
    {
        var transactionScopeHandler = GetInstance<ITransactionScopeHandler>();
        await transactionScopeHandler.RunInTransactionScope(async () => await QueryAsync(async ctx =>
        {
            ctx.Add(new TestEntity { Name = "Test" });
            await ctx.SaveChangesAsync();

            GetInstance<IBackgroundJobClient>().Enqueue(() => TestProcessor.SayHi());
        }));

        // Assert
        var testEntity = await QueryAsync(ctx => ctx.TestEntities.SingleAsync());
        testEntity.Name.Should().Be("Test");

        var jobs = await QueryJobs();
        jobs.Should().ContainSingle();

        var job = jobs[0];
        job.StateName.Should().Be("Enqueued");

        var invocationData = job.GetInvocationData();
        invocationData.Type.Should().Be(typeof(TestProcessor).AssemblyQualifiedName);
        invocationData.Method.Should().Be(nameof(TestProcessor.SayHi));
    }

    [Fact]
    public async Task CanEnqueueJob_WhileNotInTransaction()
    {
        await QueryAsync(async ctx =>
        {
            ctx.Add(new TestEntity { Name = "Test" });
            await ctx.SaveChangesAsync();

            GetInstance<IBackgroundJobClient>().Enqueue(() => TestProcessor.SayHi());
        });

        // Assert
        var testEntity = await QueryAsync(ctx => ctx.TestEntities.SingleAsync());
        testEntity.Name.Should().Be("Test");

        var jobs = await QueryJobs();
        jobs.Should().ContainSingle();

        var job = jobs[0];
        job.StateName.Should().Be("Enqueued");

        var invocationData = job.GetInvocationData();
        invocationData.Type.Should().Be(typeof(TestProcessor).AssemblyQualifiedName);
        invocationData.Method.Should().Be(nameof(TestProcessor.SayHi));
    }

    [Fact]
    public async Task EnqueueJob_WithinTransactionScope_ThrowsException_JobIsNotEnqueued()
    {
        try
        {
            var transactionScopeHandler = GetInstance<ITransactionScopeHandler>();
            await transactionScopeHandler.RunInTransactionScope(async () =>
            {
                await QueryAsync(async ctx =>
                {
                    ctx.Add(new TestEntity { Name = "Test" });
                    await ctx.SaveChangesAsync();

                    GetInstance<IBackgroundJobClient>().Enqueue(() => TestProcessor.SayHi());
                });

                throw new InvalidOperationException("Abort  transaction");
            });
        }
        catch (InvalidOperationException)
        { }

        var jobs = await QueryJobs();
        jobs.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatesOutboxJob_WithinTransactionScope()
    {
        var transactionScopeHandler = GetInstance<ITransactionScopeHandler>();
        await transactionScopeHandler.RunInTransactionScope(async () => await QueryAsync(async ctx =>
        {
            ctx.Add(new TestEntity { Name = "Test" });
            await ctx.SaveChangesAsync();

            await SendAsync(new OutOfBandCommand());
        }));

        // Assert
        var jobs = await QueryJobs();
        jobs.Should().ContainSingle();

        var job = jobs[0];
        job.StateName.Should().Be("Enqueued");

        var invocationData = job.GetInvocationData();
        invocationData.Type.Should().Be(typeof(JobProcessor).AssemblyQualifiedName);
        invocationData.Method.Should().Be(nameof(JobProcessor.ProcessAsync));
    }

    [Fact]
    public async Task CreatesOutboxJob_WhileNotInTransaction()
    {
        await QueryAsync(async ctx =>
        {
            ctx.Add(new TestEntity { Name = "Test" });
            await ctx.SaveChangesAsync();

            await SendAsync(new OutOfBandCommand());
        });

        // Assert
        var jobs = await QueryJobs();
        jobs.Should().ContainSingle();

        var job = jobs[0];
        job.StateName.Should().Be("Enqueued");

        var invocationData = job.GetInvocationData();
        invocationData.Type.Should().Be(typeof(JobProcessor).AssemblyQualifiedName);
        invocationData.Method.Should().Be(nameof(JobProcessor.ProcessAsync));
    }

    [Fact]
    public async Task OutboxJob_WithinTransactionScope_ThrowsException_JobIsNotEnqueued()
    {
        try
        {
            var transactionScopeHandler = GetInstance<ITransactionScopeHandler>();
            await transactionScopeHandler.RunInTransactionScope(async () =>
            {
                await QueryAsync(async ctx =>
                {
                    ctx.Add(new TestEntity { Name = "Test" });
                    await ctx.SaveChangesAsync();

                    await SendAsync(new OutOfBandCommand());
                });

                throw new InvalidOperationException("Abort transaction");
            });
        }
        catch (InvalidOperationException)
        { }

        var jobs = await QueryJobs();
        jobs.Should().BeEmpty();
    }

    public class TestProcessor
    {
        public static void SayHi() => Console.WriteLine("Hi");
    }

    private async Task<List<HangfireJob>> QueryJobs()
    {
        var connectionString = GetTestStore().ConnectionString;
        await using var connection = new NpgsqlConnection(connectionString);
        return connection.QueryAsync<HangfireJob>("SELECT * FROM hangfire.job").Result.ToList();
    }

    private NpgsqlDatabasePerTestStore GetTestStore() => (NpgsqlDatabasePerTestStore)GetInstance<ITestStore>();

    public class HangfireFixture : TestFixture
    {
        protected override void RegisterHangfire(IServiceCollection services) => RegisterHangfireNpgsql(services);
    }

    public class HangfireJob
    {
        public int Id { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; } = "";
        public string InvocationData { get; set; } = "";
        public InvocationData GetInvocationData() => JsonSerializer.Deserialize<InvocationData>(InvocationData)!;
    }

    public class InvocationData
    {
        public string Type { get; set; } = "";
        public string Method { get; set; } = "";
    }
}