// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;
using Hangfire;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Fusonic.Extensions.Hangfire.Tests;

public class DecoratorTests : TestBase
{
    private readonly IBackgroundJobClient jobClient;

    public DecoratorTests(TestFixture testFixture) : base(testFixture)
    {
        jobClient = GetInstance<IBackgroundJobClient>();
        jobClient.ClearSubstitute();
    }

    [Fact]
    public async Task OnlyJobProcessorShouldBeCalled_IfOutOfBandAttributeHasBeenApplied_ToCommandHandler()
    {
        var handled = false;
        OutOfBandCommandHandler.Handled += (_, _) => handled = true;

        var handler = GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
        await handler.Handle(new OutOfBandCommand(), CancellationToken.None);

        jobClient.ReceivedWithAnyArgs().Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        handled.Should().BeFalse("Handle must not be called if handler is marked for out of band processing.");
    }

    [Fact]
    public async Task OnlyJobProcessorShouldBeCalled_IfOutOfBandAttributeHasBeenApplied_ToNotificationHandler()
    {
        var outOfBandHandled = false;
        OutOfBandNotificationHandler.Handled += (_, _) => outOfBandHandled = true;

        var syncHandled = false;
        OutOfBandNotificationHandlerWithoutAttribute.Handled += (_, _) => syncHandled = true;

        var handlers = Fixture.Container.GetAllInstances<INotificationHandler<OutOfBandNotification>>().ToList();
        handlers.Should().HaveCount(2);

        foreach (var handler in handlers)
        {
            await handler.Handle(new OutOfBandNotification(), CancellationToken.None);
        }

        jobClient.ReceivedWithAnyArgs(1).Enqueue<JobProcessor>(x => x.ProcessAsync(null!, null!));
        outOfBandHandled.Should().BeFalse("Handle must not be called if handler is marked for out of band processing.");
        syncHandled.Should().BeTrue();
    }
}
