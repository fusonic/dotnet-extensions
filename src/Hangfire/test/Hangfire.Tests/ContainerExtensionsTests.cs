﻿using Fusonic.Extensions.Hangfire;
using MediatR;
using NSubstitute;
using SimpleInjector;
using Xunit;

namespace Hangfire.Tests
{
    public class ContainerExtensionsTests
    {
        private Container Container { get; } = new Container();

        public ContainerExtensionsTests()
        {
            Container.Register(typeof(IRequestHandler<,>), new[] { typeof(OutOfBandCommandHandler), typeof(CommandHandler) });
            Container.Register(typeof(INotificationHandler<>), new[] { typeof(OutOfBandNotificationHandler), typeof(NotificationHandler) });
            Container.RegisterInstance(Substitute.For<IBackgroundJobClient>());
            Container.Options.ResolveUnregisteredConcreteTypes = false;
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

        private class CustomJobProcessor : JobProcessor
        {
            public CustomJobProcessor(Container container) : base(container)
            { }
        }
    }
}