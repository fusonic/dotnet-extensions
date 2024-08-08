// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using FluentAssertions;
using Fusonic.Extensions.Mediator.Tests.Notifications;
using Fusonic.Extensions.Mediator.Tests.Requests;
using NSubstitute;
using Xunit;

namespace Fusonic.Extensions.Mediator.Tests;

public class MediatorTests(TestFixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task CanSendCommandsWithNoReturnValue()
    {
        var applicationState = new EnableStateCommand.ApplicationState(false);
        await SendAsync(new EnableStateCommand(applicationState));
        applicationState.State.Should().BeTrue();
    }

    [Fact]
    public async Task CanSendCommandsWithReturnValue()
    {
        var result = await SendAsync(new MultiplyCommand(3, 4));
        result.Value.Should().Be(12);
    }

    [Fact]
    public async Task CanSendQuery()
    {
        const string echo = "Hello World!";
        var result = await SendAsync(new EchoQuery(echo));
        result.Value.Should().Be(echo);
    }

    [Fact]
    public async Task UsesIRequest_CanSendQuery()
    {
        const string echo = "Hello World!";
        // ReSharper disable once RedundantCast
        var result = await SendAsync((IRequest<EchoQuery.Result>)new EchoQuery(echo));
        result.Value.Should().Be(echo);
    }

    [Fact]
    public async Task CanPublishNotificationsToMultipleHandlers()
    {
        var applicationState = new UserRegistered.ApplicationState(false, false);
        var notification = new UserRegistered("testuser", applicationState);

        await PublishAsync(notification);

        applicationState.FreeTrialStarted.Should().BeTrue();
        applicationState.UserWelcomed.Should().BeTrue();
    }

    [Fact]
    public async Task UsesINotification_CanPublishNotificationsToMultipleHandlers()
    {
        var applicationState = new UserRegistered.ApplicationState(false, false);
        var notification = new UserRegistered("testuser", applicationState);

        await PublishAsync((INotification)notification);

        applicationState.FreeTrialStarted.Should().BeTrue();
        applicationState.UserWelcomed.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateAsyncEnumerable()
    {
        var i = 1;
        await foreach (var item in CreateAsyncEnumerable(new ExportDataQuery()))
        {
            item.Should().BeEquivalentTo(new ExportDataQuery.Result(i, i.ToString(CultureInfo.InvariantCulture)));
            i++;
        }
    }

    [Fact]
    public async Task UsesIAsyncEnumerableRequest_CanCreateAsyncEnumerable()
    {
        var i = 1;
        // ReSharper disable once RedundantCast
        await foreach (var item in CreateAsyncEnumerable((IAsyncEnumerableRequest<ExportDataQuery.Result>)new ExportDataQuery()))
        {
            item.Should().BeEquivalentTo(new ExportDataQuery.Result(i, i.ToString(CultureInfo.InvariantCulture)));
            i++;
        }
    }

    [Fact]
    public async Task MockedRequestHandler_ReceivesCall()
    {
        // Arrange
        var handler = Substitute.For<IRequestHandler<EchoQuery, EchoQuery.Result>>();
        var query = new EchoQuery("Hi");
        
        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        await handler.Received(1).Handle(Arg.Is<EchoQuery>(q => ReferenceEquals(q, query)), CancellationToken.None);
    }

    [Fact]
    public async Task MockedNotificationHandler_ReceivesCall()
    {
        // Arrange
        var handler = Substitute.For<INotificationHandler<UserRegistered>>();
        var notification = new UserRegistered("testuser", new UserRegistered.ApplicationState(false, false));

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await handler.Received(1).Handle(Arg.Is<UserRegistered>(n => ReferenceEquals(n, notification)), CancellationToken.None);
    }

    [Fact]
    public void MockedAsyncEnumerableHandler_ReceivesCall()
    {
        // Arrange
        var handler = Substitute.For<IAsyncEnumerableRequestHandler<ExportDataQuery, ExportDataQuery.Result>>();
        var query = new ExportDataQuery();

        // Act
        _ = handler.Handle(query, CancellationToken.None);

        // Assert
        _ = handler.Received(1).Handle(Arg.Is<ExportDataQuery>(q => ReferenceEquals(q, query)), CancellationToken.None);
    }
}
