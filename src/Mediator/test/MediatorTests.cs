// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using FluentAssertions;
using Fusonic.Extensions.Mediator.Tests.Notifications;
using Fusonic.Extensions.Mediator.Tests.Requests;
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
    public async Task CanPublishNotificationsToMultipleHandlers()
    {
        var applicationState = new UserRegistered.ApplicationState(false, false);
        var notification = new UserRegistered("testuser", applicationState);

        await PublishAsync(notification);

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
}
