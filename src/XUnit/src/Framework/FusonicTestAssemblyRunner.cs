// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

public class FusonicTestAssemblyRunner : XunitTestAssemblyRunner
{
    private bool isInitialized;
    private SemaphoreSlim? testCollectionSemaphore;

    public FusonicTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    { }

    protected override Task AfterTestAssemblyStartingAsync()
    {
        Init();
        return base.AfterTestAssemblyStartingAsync();
    }

    private void Init()
    {
        if (isInitialized)
            return;

        var maxParallelTests = 0;

        // Try get maxParallelTests value from the environment
        var env = Environment.GetEnvironmentVariable("MAX_PARALLEL_TESTS")
            ?? Environment.GetEnvironmentVariable("MAX_TEST_CONCURRENCY"); // TODO: Remove support for this var with v7.0. It's here for backwards compatibility, as it was named that way in the v6 docs.
        if (env != null && int.TryParse(env, out var max))
        {
            maxParallelTests = max;
        }
        // Take value from assembly attribute setting, which defaults to 0.
        else
        {
            var fusonicTestFrameworkAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(FusonicTestFrameworkAttribute)).Single();
            maxParallelTests = fusonicTestFrameworkAttribute.GetNamedArgument<int>(nameof(FusonicTestFrameworkAttribute.MaxParallelTests));
        }

        if (maxParallelTests == 0)
            maxParallelTests = Environment.ProcessorCount;

        if (maxParallelTests > 0)
            testCollectionSemaphore = new SemaphoreSlim(maxParallelTests);

        isInitialized = true;
    }

    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        if (testCollectionSemaphore == null)
            return await base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);

        try
        {
            var cancellationToken = cancellationTokenSource.Token;
            await testCollectionSemaphore.WaitAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var result = await base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);
            return result;
        }
        finally
        {
            testCollectionSemaphore.Release();
        }
    }
}