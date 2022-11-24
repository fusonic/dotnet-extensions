// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

public class FusonicTestFramework : XunitTestFramework
{
    public FusonicTestFramework(IMessageSink messageSink) : base(messageSink)
    { }

    /// <summary>
    /// This is more or less the starting point of the whole overwrite-pipe. It runs through a lot of overwritten classes only returning our Fusonic-Versions, that don't have any extra logic.
    /// The FusonicTestRunner is the one that creates the test context and calls our own before/after test attributes.
    /// </summary>
    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        => new FusonicTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

    /// <summary>
    /// The executor uses another assembly runner that can limit the maximum amount of parallel tests, in addition to the maximum amount of threads provided by XUnit.
    /// </summary>
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new FusonicTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}