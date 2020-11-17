// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    public class FusonicTestCase : XunitTestCase
    {
#pragma warning disable 618
        //ctor is marked with obsolete, but is required by serializer. See base class.
        public FusonicTestCase()
        { }
#pragma warning restore 618

        public FusonicTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            object[]? testMethodArguments = null) : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        { }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new FusonicTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource)
               .RunAsync();
        }
    }
}