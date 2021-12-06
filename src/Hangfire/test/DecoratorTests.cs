// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Hangfire;
using MediatR;
using NSubstitute;
using Xunit;

namespace Fusonic.Extensions.Hangfire.Tests;

public class DecoratorTests : TestBase
{
    [Fact]
    public async Task OnlyJobProcessorShouldBeCalled_IfOutOfBandAttributeHasBeenApplied_ToCommandHandler()
    {
        var handled = false;
        OutOfBandCommandHandler.Handled += (_, __) => handled = true;

        var handler = Container.GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
        await handler.Handle(new OutOfBandCommand(), CancellationToken.None);

        JobClient.ReceivedWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.False(handled, "Handle must not be called if handler is marked for out of band processing.");
    }

    [Fact]
    public async Task OnlyJobProcessorShouldBeCalled_IfOutOfBandAttributeHasBeenApplied_ToNotificationHandler()
    {
        var outOfBandHandled = false;
        OutOfBandNotificationHandler.Handled += (_, __) => outOfBandHandled = true;

        var syncHandled = false;
        OutOfBandNotificationHandlerWithoutAttribute.Handled += (_, __) => syncHandled = true;

        var handlers = Container.GetAllInstances<INotificationHandler<OutOfBandNotification>>().ToList();
        Assert.Equal(2, handlers.Count);

        foreach (var handler in handlers)
        {
            await handler.Handle(new OutOfBandNotification(), CancellationToken.None);
        }

        JobClient.ReceivedWithAnyArgs(1).Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        Assert.False(outOfBandHandled, "Handle must not be called if handler is marked for out of band processing.");
        Assert.True(syncHandled);
    }
}
