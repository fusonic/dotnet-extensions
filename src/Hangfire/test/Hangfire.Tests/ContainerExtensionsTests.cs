using Fusonic.Extensions.Hangfire;
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
            Container.RegisterOutOfBandDecorators();
            Container.Register(typeof(IRequestHandler<,>), new[] { typeof(OutOfBandCommandHandler), typeof(CommandHandler) });
            Container.Register(typeof(INotificationHandler<>), new[] { typeof(OutOfBandNotificationHandler), typeof(NotificationHandler) });
            Container.RegisterInstance(Substitute.For<IBackgroundJobClient>());
        }

        [Fact]
        public void OutOfBandRequestHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
        {
            var decorated = Container.GetInstance<IRequestHandler<OutOfBandCommand, Unit>>();
            Assert.Equal(typeof(OutOfBandRequestHandlerDecorator<OutOfBandCommand>), decorated.GetType());

            var handler = Container.GetInstance<IRequestHandler<Command, Unit>>();
            Assert.Equal(typeof(CommandHandler), handler.GetType());
        }

        [Fact]
        public void NotificationHandlerShouldBeResolved_IfOutOfBandAttributeHasBeenApplied()
        {
            var decorated = Container.GetInstance<INotificationHandler<OutOfBandNotification>>();
            Assert.Equal(typeof(OutOfBandNotificationHandlerDecorator<OutOfBandNotification>), decorated.GetType());

            var handler = Container.GetInstance<INotificationHandler<Notification>>();
            Assert.Equal(typeof(NotificationHandler), handler.GetType());
        }
    }
}