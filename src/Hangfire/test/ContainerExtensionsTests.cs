// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Security;
using Fusonic.Extensions.Mediator;
using Hangfire;
using NSubstitute;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace Fusonic.Extensions.Hangfire.Tests;

public class ContainerExtensionsTests : IDisposable
{
    private readonly Scope scope;
    private Container Container { get; } = new();

    public ContainerExtensionsTests()
    {
        Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        Container.Register(typeof(IRequestHandler<,>), new[] { typeof(OutOfBandCommandHandler), typeof(CommandHandler) });
        Container.Register(typeof(INotificationHandler<>), new[] { typeof(OutOfBandNotificationHandler), typeof(NotificationHandler) });
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
        Assert.Equal(typeof(OutOfBandRequestHandlerDecorator<OutOfBandCommand, JobProcessor>), decorated.GetType());

        var handler = Container.GetInstance<IRequestHandler<Command, Unit>>();
        Assert.Equal(typeof(CommandHandler), handler.GetType());
    }

    [Fact]
    public void JobProcessorMustBeResolvable()
    {
        Container.RegisterOutOfBandDecorators();
        Assert.NotNull(Container.GetInstance<JobProcessor>());
    }

    [Fact]
    public void OutOfBandRequestHandler_UsesCustomJobProcessor()
    {
        Container.RegisterOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        var decorated = Container.GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
        Assert.Equal(typeof(OutOfBandRequestHandlerDecorator<OutOfBandCommand, CustomJobProcessor>), decorated.GetType());

        var decoratedNotification = Container.GetInstance<INotificationHandler<OutOfBandNotification>>();
        Assert.Equal(typeof(OutOfBandNotificationHandlerDecorator<OutOfBandNotification, CustomJobProcessor>), decoratedNotification.GetType());
    }

    [Fact]
    public void CustomJobProcessorMustBeResolvable()
    {
        Container.RegisterOutOfBandDecorators(options => options.UseJobProcessor<CustomJobProcessor>());
        Assert.NotNull(Container.GetInstance<CustomJobProcessor>());
    }

    [Fact]
    public void NotificationHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
    {
        Container.RegisterOutOfBandDecorators();
        var decorated = Container.GetInstance<INotificationHandler<OutOfBandNotification>>();
        Assert.Equal(typeof(OutOfBandNotificationHandlerDecorator<OutOfBandNotification, JobProcessor>), decorated.GetType());

        var handler = Container.GetInstance<INotificationHandler<Notification>>();
        Assert.Equal(typeof(NotificationHandler), handler.GetType());
    }

    [Fact]
    public void UserAccessorDecoratorShouldBeApplied()
    {
        Container.RegisterOutOfBandDecorators();
        Assert.IsType<HangfireUserAccessorDecorator>(Container.GetInstance<IUserAccessor>());
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
