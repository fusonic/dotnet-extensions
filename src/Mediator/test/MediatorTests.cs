// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using FluentAssertions;
using Fusonic.Extensions.Mediator.Tests.Notifications;
using Fusonic.Extensions.Mediator.Tests.Requests;
using Xunit;

namespace Fusonic.Extensions.Mediator.Tests;

public class MediatorTests : TestBase
{
    private readonly IMediator mediator;

    public MediatorTests(TestFixture fixture) : base(fixture)
    {
        mediator = GetInstance<IMediator>();
    }

    [Fact]
    public async Task Send_CommandWithNoReturnValue()
    {
        var applicationState = new EnableStateCommand.ApplicationState(false);
        var command = (IRequest)new EnableStateCommand(applicationState);

        await mediator.Send(command);

        applicationState.State.Should().BeTrue();
    }

    [Fact]
    public async Task Send_CommandWithReturnValue()
    {
        var command = new MultiplyCommand(3, 4);

        var result = await mediator.Send(command);

        result.Value.Should().Be(12);
    }

    [Fact]
    public async Task Send_Query()
    {
        const string echo = "Hello World!";
        var command = new EchoQuery(echo);

        var result = await mediator.Send(command);

        result.Value.Should().Be(echo);
    }

    [Fact]
    public async Task Publish_Notification_ReceivedByMultipleHandlers()
    {
        var applicationState = new UserRegistered.ApplicationState(false, false);
        var notification = new UserRegistered("testuser", applicationState);

        await mediator.Publish(notification);

        applicationState.FreeTrialStarted.Should().BeTrue();
        applicationState.UserWelcomed.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsyncEnumerable()
    {
        var i = 1;
        var request = new ExportDataQuery();

        var enumerable = mediator.CreateAsyncEnumerable(request);

        await foreach (var item in enumerable)
        {
            item.Should().BeEquivalentTo(new ExportDataQuery.Result(i, i.ToString(CultureInfo.InvariantCulture)));
            i++;
        }
    }
}
