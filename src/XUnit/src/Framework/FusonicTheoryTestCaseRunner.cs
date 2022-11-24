// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

public class FusonicTheoryTestCaseRunner : XunitTheoryTestCaseRunner
{
    public FusonicTheoryTestCaseRunner(
        IXunitTestCase testCase,
        string displayName,
        string skipReason,
        object[] constructorArguments,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource) : base(testCase, displayName, skipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource)
    { }

    protected override XunitTestRunner CreateTestRunner(
        ITest test,
        IMessageBus messageBus,
        Type testClass,
        object[] constructorArguments,
        MethodInfo testMethod,
        object[] testMethodArguments,
        string skipReason,
        IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        return new FusonicTestRunner(test,
            messageBus,
            testClass,
            constructorArguments,
            testMethod,
            testMethodArguments,
            skipReason,
            beforeAfterAttributes,
            new ExceptionAggregator(aggregator),
            cancellationTokenSource);
    }
}
