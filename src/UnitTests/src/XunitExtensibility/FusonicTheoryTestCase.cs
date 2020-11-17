// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    public class FusonicTheoryTestCase : XunitTheoryTestCase
    {
#pragma warning disable 618
        //ctor is marked with obsolete, but is required by serializer. See base class.
        public FusonicTheoryTestCase()
        { }
#pragma warning restore 618

        public FusonicTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod) : base(
            diagnosticMessageSink,
            defaultMethodDisplay,
            defaultMethodDisplayOptions,
            testMethod)
        { }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new FusonicTheoryTestCaseRunner(this,
                    DisplayName,
                    SkipReason,
                    constructorArguments,
                    diagnosticMessageSink,
                    messageBus,
                    aggregator,
                    cancellationTokenSource)
               .RunAsync();
        }
    }
}