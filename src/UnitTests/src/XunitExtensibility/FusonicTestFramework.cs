// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    public class FusonicTestFramework : XunitTestFramework
    {
        public FusonicTestFramework(IMessageSink messageSink) : base(messageSink)
        { }

        /// <summary>
        /// This is more or less the starting point of the whole overwrite-pipe. It runs through a lot of overwritten classes only returning our Fusonic-Versions, that don't have any extra logic.
        /// The FusonicTestRunner is the one that does stuff.
        /// </summary>
        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new FusonicTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }
    }
}