// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

public class FusonicTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public FusonicTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    { }

    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new FusonicTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}