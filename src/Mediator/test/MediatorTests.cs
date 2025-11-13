// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Transactions;
using Fusonic.Extensions.Mediator.Tests.Notifications;
using Fusonic.Extensions.Mediator.Tests.Requests;
using Fusonic.Extensions.UnitTests;
using NSubstitute;

namespace Fusonic.Extensions.Mediator.Tests;

public class ServiceProviderMediatorNonTransactionalTests(ServiceProviderMediatorNonTransactionalTestFixture fixture) : MediatorTests<ServiceProviderMediatorNonTransactionalTestFixture>(fixture);
public class ServiceProviderMediatorTransactionalTests(ServiceProviderMediatorTransactionalTestFixture fixture) : MediatorTests<ServiceProviderMediatorTransactionalTestFixture>(fixture);
public class SimpleInjectorMediatorNonTransactionalTests(SimpleInjectorMediatorNonTransactionalTestFixture fixture) : MediatorTests<SimpleInjectorMediatorNonTransactionalTestFixture>(fixture);
public class SimpleInjectorMediatorTransactionalTests(SimpleInjectorMediatorTransactionalTestFixture fixture) : MediatorTests<SimpleInjectorMediatorTransactionalTestFixture>(fixture);

public abstract class MediatorTests<T>(T fixture) : TestBase<T>(fixture)
    where T : class, IDependencyInjectionTestFixture, IMediatorTestFixture
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

#pragma warning disable IDE0004 // Redundant cast
    [Fact]
    public async Task UsesIRequest_CanSendQuery()
    {
        const string echo = "Hello World!";
        var result = await SendAsync((IRequest<EchoQuery.Result>)new EchoQuery(echo));
        result.Value.Should().Be(echo);
    }
#pragma warning restore IDE0004 // Redundant cast

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
        await foreach (var item in GetInstance<IMediator>().CreateAsyncEnumerable(new ExportDataQuery(), TestContext.Current.CancellationToken))
        {
            item.Should().BeEquivalentTo(new ExportDataQuery.Result(i, i.ToString(CultureInfo.InvariantCulture)));
            i++;
        }
    }

#pragma warning disable IDE0004 // Redundant cast
    [Fact]
    public async Task UsesIAsyncEnumerableRequest_CanCreateAsyncEnumerable()
    {
        var i = 1;
        await foreach (var item in GetInstance<IMediator>().CreateAsyncEnumerable((IAsyncEnumerableRequest<ExportDataQuery.Result>)new ExportDataQuery(), TestContext.Current.CancellationToken))
        {
            item.Should().BeEquivalentTo(new ExportDataQuery.Result(i, i.ToString(CultureInfo.InvariantCulture)));
            i++;
        }
    }
#pragma warning restore IDE0004 // Redundant cast

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

    [Fact]
    public async Task CalledWithinTransaction_IfTransactionDecoratorIsEnabled()
    {
        var act = () => SendAsync(new CheckTransaction(ExpectTransaction: Fixture.EnableTransactionalDecorators));
        await act.Should().NotThrowAsync();
    }
}

public record CheckTransaction(bool ExpectTransaction) : ICommand
{
    public class Handler : IRequestHandler<CheckTransaction>
    {
        public Task<Unit> Handle(CheckTransaction request, CancellationToken cancellationToken)
        {
            var hasTransaction = Transaction.Current != null;
            if (hasTransaction != request.ExpectTransaction)
                throw new InvalidOperationException($"Expected transaction: {request.ExpectTransaction}, but was: {hasTransaction}");
            return Task.FromResult(Unit.Value);
        }
    }
}
