// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;
using Fusonic.Extensions.XUnit.Framework;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

/// <summary> Overwrite this attribute if you create a new database adapter. The overwriting attribute will be used as identifier for the provider to be used. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class DatabaseProviderAttribute : BeforeAfterTestInvokeAttribute
{
    public sealed override void Before(MethodInfo methodUnderTest)
        => DatabaseTestContext.CurrentProviderAttribute.Value = this;

    public sealed override void After(MethodInfo methodUnderTest)
        => DatabaseTestContext.CurrentProviderAttribute.Value = null;
}