using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    public abstract class DatabaseUnitTest<TDbContext, TFixture> : UnitTest<TFixture>
        where TDbContext : DbContext
        where TFixture : DatabaseFixture<TDbContext>
    {
        protected DatabaseUnitTest(TFixture fixture) : base(fixture)
        {
            Query(dbContext =>
            {
                var provider = GetInstance<ITestDatabaseProvider<TDbContext>>();
                provider.SeedDb(dbContext);
            });
        }

        /// <summary> Executes a query in an own scope. </summary>
        [DebuggerStepThrough]
        protected void Query(Action<TDbContext> query)
        {
            Query(ctx =>
            {
                query(ctx);
                return true;
            });
        }

        /// <summary> Executes a query in an own scope. </summary>
        [DebuggerStepThrough]
        protected TResult Query<TResult>(Func<TDbContext, TResult> query)
        {
            var resultType = typeof(TResult);
            if (resultType == typeof(Task) || resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
                throw new ArgumentException("This is the wrong method for async queries. Use QueryAsync() instead.");

            return Scoped(() =>
            {
                using var dbContext = GetInstance<TDbContext>();
                return query(dbContext);
            });
        }

        /// <summary> Executes a query in an own scope. </summary>
        [DebuggerStepThrough]
        protected Task QueryAsync(Func<TDbContext, Task> query)
        {
            return QueryAsync(async ctx =>
            {
                await query(ctx);
                return true;
            });
        }

        /// <summary> Executes a query in an own scope. </summary>
        [DebuggerStepThrough]
        protected Task<TResult> QueryAsync<TResult>(Func<TDbContext, Task<TResult>> query)
        {
            return ScopedAsync(async () =>
            {
                await using var dbContext = GetInstance<TDbContext>();
                return await query(dbContext);
            });
        }
        
        public override void Dispose()
        {
            Query(dbContext =>
            {
                var provider = GetInstance<ITestDatabaseProvider<TDbContext>>();
                provider.DropDb(dbContext);
            });
            base.Dispose();
        }
    }
}