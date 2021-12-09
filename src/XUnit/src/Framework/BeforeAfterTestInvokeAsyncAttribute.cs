// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;

namespace Fusonic.Extensions.XUnit.Framework;

/// <summary>
/// Base attribute which indicates a test method interception. This allows code to be run before and after the test is run.
///
/// The difference to the Xunit.Sdk.BeforeAfterTestAttribute is that the xunit version executes the 'Before' after the test class is created and the constructor ran.
/// This version runs before the test class is created.
///
/// Limitations: You cannot use this to do changes to the test context. The test context is AsyncLocal. Due to the nature of AsyncLocal, which only propagates the context
/// from parent to child, you will loose all information you set in the test context after the returned Task is awaited (which is before executing the test in case of BeforeAsync).
///
/// If you need to set something in the test context, use the sync attribute BeforeAfterTestInvokeAttribute instead.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class BeforeAfterTestInvokeAsyncAttribute : Attribute
{
    /// <summary>
    /// This method is called after the test method is executed.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual Task BeforeAsync(MethodInfo methodUnderTest) => Task.CompletedTask;

    /// <summary>
    /// This method is called before the test class is created and the method is executed.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual Task AfterAsync(MethodInfo methodUnderTest) => Task.CompletedTask;
}
