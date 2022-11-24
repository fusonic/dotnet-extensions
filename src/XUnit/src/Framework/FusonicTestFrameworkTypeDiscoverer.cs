// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

public class FusonicTestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
{
    public Type GetTestFrameworkType(IAttributeInfo attribute)
        => typeof(FusonicTestFramework);
}
