// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;
using NSubstitute;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.Extensions.Hangfire.Tests;

public class ContainerExtensionsTests : IDisposable
{
    private readonly Scope scope;
    private Container Container { get; } = new();

    public ContainerExtensionsTests()
    {
        Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        Container.Register(typeof(IRequestHandler<,>), [typeof(OutOfBandCommandHandler), typeof(CommandHandler)]);
        Container.Register(typeof(INotificationHandler<>), [typeof(OutOfBandNotificationHandler), typeof(NotificationHandler)]);
        Container.RegisterInstance(Substitute.For<IBackgroundJobClient>());
        Container.RegisterInstance(Substitute.For<IUserAccessor>());
        Container.Options.ResolveUnregisteredConcreteTypes = false;

        scope = AsyncScopedLifestyle.BeginScope(Container);
    }

    [Fact]
    public void OutOfBandRequestHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
    {
        Container.RegisterOutOfBandDecorators();
        var decorated = Container.GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
        decorated.Should().BeOfType<OutOfBandRequestHandlerDecorator<OutOfBandCommand, JobProcessor>>();

        var handler = Container.GetInstance<IRequestHandler<Command, Unit>>();
        handler.Should().BeOfType<CommandHandler>();
    }

    [Fact]
    public void JobProcessorMustBeResolvable()
    {
        Container.RegisterOutOfBandDecorators();
        Container.GetInstance<JobProcessor>().Should().NotBeNull();
    }

    [Fact]
    public void OutOfBandRequestHandler_UsesCustomJobProcessor()
    {
        Container.RegisterOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        var decorated = Container.GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
        decorated.Should().BeOfType<OutOfBandRequestHandlerDecorator<OutOfBandCommand, CustomJobProcessor>>();

        var decoratedNotification = Container.GetInstance<INotificationHandler<OutOfBandNotification>>();
        decoratedNotification.Should().BeOfType<OutOfBandNotificationHandlerDecorator<OutOfBandNotification, CustomJobProcessor>>();
    }

    [Fact]
    public void CustomJobProcessorMustBeResolvable()
    {
        Container.RegisterOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        Container.GetInstance<CustomJobProcessor>().Should().NotBeNull();
    }

    [Fact]
    public void NotificationHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
    {
        Container.RegisterOutOfBandDecorators();
        var decorated = Container.GetInstance<INotificationHandler<OutOfBandNotification>>();
        decorated.Should().BeOfType<OutOfBandNotificationHandlerDecorator<OutOfBandNotification, JobProcessor>>();

        var handler = Container.GetInstance<INotificationHandler<Notification>>();
        handler.Should().BeOfType<NotificationHandler>();
    }

    [Fact]
    public void UserAccessorDecoratorShouldBeApplied()
    {
        Container.RegisterOutOfBandDecorators();
        Container.GetInstance<IUserAccessor>().Should().BeOfType<HangfireUserAccessorDecorator>();
    }

    private sealed class CustomJobProcessor(Container container) : JobProcessor(container)
    {
    }

    public void Dispose()
    {
        scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
