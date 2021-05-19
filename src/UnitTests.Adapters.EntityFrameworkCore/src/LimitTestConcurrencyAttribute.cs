using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.UnitTests.XunitExtensibility;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    //TODO 6.0: Move this logic/idea to the executor (-> FusonicTestFramework.CreateExecutor) to avoid falsifying execution times and just not execute limited tests instead.
    [AttributeUsage(AttributeTargets.Class)]
    internal class LimitTestConcurrencyAttribute : BeforeAfterTestInvokeAsyncAttribute
    {
        private static SemaphoreSlim? semaphore;
        private static bool testExecuted;
        private static int maxConcurrency; // Default = 0 = Processor count

        static LimitTestConcurrencyAttribute()
        {
            semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        }

        internal static int MaxConcurrency
        {
            set
            {
                if (value == maxConcurrency)
                    return;

                if (testExecuted)
                    throw new InvalidOperationException("Cannot change max concurrency once the tests started.");

                maxConcurrency = value;
                if (maxConcurrency < 0)
                    semaphore = null;

                else if (maxConcurrency == 0)
                    semaphore = new(Environment.ProcessorCount);

                else
                    semaphore = new(maxConcurrency);
            }
        }

        public override Task BeforeAsync(MethodInfo methodUnderTest)
        {
            testExecuted = true;
            return semaphore?.WaitAsync() ?? Task.CompletedTask;
        }

        public override Task AfterAsync(MethodInfo methodUnderTest)
        {
            semaphore?.Release();
            return Task.CompletedTask;
        }
    }
}