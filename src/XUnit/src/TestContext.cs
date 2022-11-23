// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Collections;
using System.Reflection;
using Xunit.Abstractions;

namespace Fusonic.Extensions.XUnit;

/// <summary> Provides a test context per method. </summary>
public static class TestContext
{
    private static readonly AsyncLocal<InternalTestContext?> InternalContext = new();
    internal static InternalTestContext? CurrentContext => InternalContext.Value;

    /// <summary> Gets the TestOutputHelper. There are also additional shortcuts to this with the WriteLine-Methods if you just want to log something to the test output. </summary>
    public static ITestOutputHelper Out => CurrentContext!.OutputHelper;

    /// <summary> Gets the currently executing test method. </summary>
    public static MethodInfo TestMethod => CurrentContext!.TestMethod;

    /// <summary> Gets the currently executing test class. </summary>
    public static Type TestClass => CurrentContext!.TestClass;

    /// <summary> Allows you to put objects in the test context. </summary>
    public static IDictionary Items => CurrentContext!.Items;

    /// <summary> Writes a message to the test output. </summary>
    public static void WriteLine(string message) => Out.WriteLine(message);

    /// <summary> Writes a formatted message to the test output. </summary>
    public static void WriteLine(string format, params object[] args) => Out.WriteLine(format, args);

    /// <summary>
    /// Gets if the test context is set. Usually this is always true. You just might want to check it in framework code to verify that the test assembly uses
    /// the FusonicTestFramework.
    /// </summary>
    public static bool IsSet => InternalContext.Value != null;

    internal static IDisposable Create(ITestOutputHelper testOutputHelper, MethodInfo testMethod, Type testClass)
    {
        InternalContext.Value = new InternalTestContext(testOutputHelper, testMethod, testClass);

        return InternalContext.Value;
    }

    internal sealed class InternalTestContext : IDisposable
    {
        private IDictionary? items;

        public ITestOutputHelper OutputHelper { get; }
        public MethodInfo TestMethod { get; }
        public Type TestClass { get; }
        public IDictionary Items => items ??= new Hashtable();

        public InternalTestContext(ITestOutputHelper outputHelper, MethodInfo testMethod, Type testClass)
        {
            OutputHelper = outputHelper;
            TestMethod = testMethod;
            TestClass = testClass;
        }

        public void Dispose() => InternalContext.Value = null;
    }
}