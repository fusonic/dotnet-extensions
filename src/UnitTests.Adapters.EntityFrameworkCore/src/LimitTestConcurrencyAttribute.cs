using System.Reflection;
using Fusonic.Extensions.XUnit.Framework;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

[Obsolete("This attribute is obsolete and will be removed in v7.0. The maximum number of tests can now be set in the attribute 'FusonicTestFramework' on assembly level.")]
[AttributeUsage(AttributeTargets.Class)]
internal sealed class LimitTestConcurrencyAttribute : BeforeAfterTestInvokeAsyncAttribute
{
    private static SemaphoreSlim? semaphore;
    private static bool testExecuted;

    /// <summary>
    /// Default = -1 = disabled
    /// </summary>
    private static int maxConcurrency = -1;

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
        //Note: While the test method was reported as started when reaching this point, waiting for the semaphore
        //      does not affect the measured execution time or the test timeout.
        testExecuted = true;
        return semaphore?.WaitAsync() ?? Task.CompletedTask;
    }

    public override Task AfterAsync(MethodInfo methodUnderTest)
    {
        semaphore?.Release();
        return Task.CompletedTask;
    }
}