// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;

namespace Fusonic.Extensions.XUnit.Framework;

/// <summary>
/// Base attribute which indicates a test method interception. This allows code to be run before and after the test is run.
///
/// The difference to the Xunit.Sdk.BeforeAfterTestAttribute is that the XUnit version executes the 'Before' after the test class is created and the constructor ran.
/// This version runs before the test class is created, allowing to create something like a test context before the constructor gets called.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class BeforeAfterTestInvokeAttribute : Attribute
{
    /// <summary>
    /// This method is called before the test class is created and the method is executed.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual void Before(MethodInfo methodUnderTest)
    { }

    /// <summary>
    /// This method is called after the test method is executed.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual void After(MethodInfo methodUnderTest)
    { }
}
