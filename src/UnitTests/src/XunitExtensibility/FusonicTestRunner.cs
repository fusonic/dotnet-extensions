// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    /// <summary>
    /// Runs the test methods. This class is additionally responsible for creating the TestContext, handling the test output and running the BeforeAfterInvokeAttribute-Methods.
    /// </summary>
    public class FusonicTestRunner : XunitTestRunner
    {
        private readonly List<BeforeAfterTestInvokeAttribute> beforeAfterInvokeAttributes = new();
        private readonly List<BeforeAfterTestInvokeAsyncAttribute> beforeAfterInvokeAsyncAttributes = new();
        private readonly ITestOutputHelper? testOutputHelper;

        public FusonicTestRunner(
            ITest test,
            IMessageBus messageBus,
            Type testClass,
            object[] constructorArguments,
            MethodInfo testMethod,
            object[] testMethodArguments,
            string skipReason,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            //First get the attributes on the class, then on the method. Method attributes should get executed after the class attributes.
            beforeAfterInvokeAttributes.AddRange(testClass.GetCustomAttributes<BeforeAfterTestInvokeAttribute>(inherit: true));
            beforeAfterInvokeAttributes.AddRange(testMethod.GetCustomAttributes<BeforeAfterTestInvokeAttribute>());

            beforeAfterInvokeAsyncAttributes.AddRange(testClass.GetCustomAttributes<BeforeAfterTestInvokeAsyncAttribute>(inherit: true));
            beforeAfterInvokeAsyncAttributes.AddRange(testMethod.GetCustomAttributes<BeforeAfterTestInvokeAsyncAttribute>());

            //If there's a ITestOutputHelper in the ctor, we use that one instead of creating an own.
            testOutputHelper = constructorArguments.OfType<ITestOutputHelper>().FirstOrDefault();
        }

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            //if the output helper wasn't in the ctor, we create our own.
            TestOutputHelper? ownOutputHelper = null;
            if (testOutputHelper == null)
            {
                ownOutputHelper = new TestOutputHelper();
                ownOutputHelper.Initialize(MessageBus, Test);
            }

            using (TestContext.Create(testOutputHelper ?? ownOutputHelper!, TestMethod, TestClass))
            {
                BeforeTestCaseInvoked();
                await BeforeTestCaseInvokedAsync();

                var result = await base.InvokeTestAsync(aggregator);

                await AfterTestCaseInvokedAsync();
                AfterTestCaseInvoked();

                //if we own the output helper, set the output in the result
                if (ownOutputHelper != null)
                {
                    result = new Tuple<decimal, string>(result.Item1, ownOutputHelper.Output);
                    ownOutputHelper.Uninitialize();
                }

                return result;
            }
        }

        private void BeforeTestCaseInvoked()
        {
            foreach (var attribute in beforeAfterInvokeAttributes)
            {
                attribute.Before(TestMethod);
            }
        }

        private void AfterTestCaseInvoked()
        {
            foreach (var attribute in beforeAfterInvokeAttributes)
            {
                attribute.After(TestMethod);
            }
        }

        private async Task BeforeTestCaseInvokedAsync()
        {
            foreach (var attribute in beforeAfterInvokeAsyncAttributes)
            {
                await attribute.BeforeAsync(TestMethod);
            }
        }

        private async Task AfterTestCaseInvokedAsync()
        {
            foreach (var attribute in beforeAfterInvokeAsyncAttributes)
            {
                await attribute.AfterAsync(TestMethod);
            }
        }
    }
}