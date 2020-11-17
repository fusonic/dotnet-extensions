// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    public class FusonicTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
    {
        public FusonicTestFrameworkDiscoverer(
            IAssemblyInfo assemblyInfo,
            ISourceInformationProvider sourceProvider,
            IMessageSink diagnosticMessageSink,
            IXunitTestCollectionFactory? collectionFactory = null) : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
        {
            DiscovererTypeCache[typeof(FactAttribute)] = typeof(FusonicFactDiscoverer);
            DiscovererTypeCache[typeof(TheoryAttribute)] = typeof(FusonicTheoryDiscoverer);
        }
    }
}