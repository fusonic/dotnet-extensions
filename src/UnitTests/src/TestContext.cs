// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;

namespace Fusonic.Extensions.UnitTests
{
    /// <summary> Provides a test context per method. Will probably be obsolete with Xunit 3 :) </summary>
    public static class TestContext
    {
        private static readonly AsyncLocal<InternalTestContext?> context = new AsyncLocal<InternalTestContext?>();
        internal static InternalTestContext? Current => context.Value;

        /// <summary> Gets the TestOutputHelper. There are also additional shortcuts to this with the WriteLine-Methods if you just want to log something to the test output. </summary>
        public static ITestOutputHelper Out => Current!.OutputHelper;

        /// <summary> Gets the currently executing test method. </summary>
        public static MethodInfo TestMethod => Current!.TestMethod;

        /// <summary> Gets the currently executing test class. </summary>
        public static Type TestClass => Current!.TestClass;

        /// <summary> Allows you to put objects in the test context. </summary>
        public static IDictionary Items => Current!.Items;

        /// <summary> Writes a message to the test output. </summary>
        public static void WriteLine(string message) => Out.WriteLine(message);

        /// <summary> Writes a formatted message to the test output. </summary>
        public static void WriteLine(string format, params object[] args) => Out.WriteLine(format, args);

        internal static IDisposable Create(ITestOutputHelper testOutputHelper, MethodInfo testMethod, Type testClass)
        {
            context.Value = new InternalTestContext(testOutputHelper, testMethod, testClass);

            return context.Value;
        }

        internal class InternalTestContext : IDisposable
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

            public void Dispose()
            {
                context.Value = null;
            }
        }
    }
}