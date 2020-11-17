// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    public class FusonicTheoryDiscoverer : TheoryDiscoverer
    {
        public FusonicTheoryDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        { }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            object[] dataRow)
        {
            return new[] { new FusonicTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, dataRow) };
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForSkip(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            string skipReason)
        {
            return new[] { new FusonicTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod) };
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForSkippedDataRow(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            object[] dataRow,
            string skipReason)
        {
            return new[]
            {
                new FusonicSkippedDataRowTestCase(DiagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod,
                    skipReason,
                    dataRow)
            };
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            return new[] { new FusonicTheoryTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod) };
        }
    }
}