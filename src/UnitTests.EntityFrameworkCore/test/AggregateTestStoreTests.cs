// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;

namespace Fusonic.Extensions.UnitTests.EntityFrameworkCore.Tests;
public class AggregateTestStoreTests
{
    [Fact]
    public void CallsOnTestConstruction_InAllTestStores()
    {
        // Arrange
        var store1 = Substitute.For<ITestStore>();
        var store2 = Substitute.For<ITestStore>();

        var aggregateStore = new AggregateTestStore(store1, store2);

        // Act
        aggregateStore.OnTestConstruction();

        // Assert
        store1.Received(1).OnTestConstruction();
        store2.Received(1).OnTestConstruction();
    }

    [Fact]
    public async Task CallsOnTestEnd_InAllTestStores()
    {
        // Arrange
        var store1 = Substitute.For<ITestStore>();
        var store2 = Substitute.For<ITestStore>();

        var aggregateStore = new AggregateTestStore(store1, store2);

        // Act
        await aggregateStore.OnTestEnd();

        // Assert
        await store1.Received(1).OnTestEnd();
        await store2.Received(1).OnTestEnd();
    }

    [Fact]
    public async Task WhenTestEndThrowsException_CallsOnTestEnd_InAllTestStores_ThrowsAggregateException()
    {
        // Arrange
        var store1 = Substitute.For<ITestStore>();
        store1.OnTestEnd().Throws(new InvalidOperationException("Store 1"));

        var store2 = Substitute.For<ITestStore>();
        store2.OnTestEnd().Throws(new ArgumentException("Store 2"));

        var aggregateStore = new AggregateTestStore(store1, store2);

        var act = aggregateStore.OnTestEnd;

        // Act / Assert
        await act.Should()
                 .ThrowExactlyAsync<AggregateException>()
                 .Where(e => e.InnerExceptions.Count == 2)
                 .Where(e => e.InnerExceptions.Any(i => i is InvalidOperationException && i.Message == "Store 1"))
                 .Where(e => e.InnerExceptions.Any(i => i is ArgumentException && i.Message == "Store 2"));

        await store1.Received(1).OnTestEnd();
        await store2.Received(1).OnTestEnd();
    }
}
