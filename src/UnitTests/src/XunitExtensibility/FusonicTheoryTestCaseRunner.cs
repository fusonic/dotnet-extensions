using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
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
}