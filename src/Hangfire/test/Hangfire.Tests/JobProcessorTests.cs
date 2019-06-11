using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Abstractions;
using Fusonic.Extensions.Hangfire.Internal;
using MediatR;
using SimpleInjector;
using Xunit;

namespace Hangfire.Tests
{
    public class HangfireJobProcessorTests
    {
        public HangfireJobProcessorTests()
        {
            Container = new Container();
        }

        private Container Container { get; }

        protected T GetInstance<T>()
            where T : class
        {
            return Container.GetInstance<T>();
        }

        [Fact]
        public async Task CanProcessCommand()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Command(),
                HandlerType = typeof(CommandHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessNotification()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Notification(),
                HandlerType = typeof(NotificationHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessNotificationSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Notification(),
                HandlerType = typeof(SyncNotificationHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessRequest()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Request(),
                HandlerType = typeof(RequestHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessRequestSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new SyncRequest(),
                HandlerType = typeof(SyncRequestHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task RestoreCulture()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var specific = CultureInfo.CreateSpecificCulture("en-us");
            var specificUI = CultureInfo.CreateSpecificCulture("de-at");

            Container.Register(() => new CommandHandlerWithCulture(() =>
            {
                Assert.Equal(specific, CultureInfo.CurrentCulture);
                Assert.Equal(specificUI, CultureInfo.CurrentUICulture);
            }));

            CultureInfo.CurrentCulture = specific;
            CultureInfo.CurrentUICulture = specificUI;

            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new CommandWithCulture(),
                HandlerType = typeof(CommandHandlerWithCulture).AssemblyQualifiedName
            });
        }

        public class Command : ICommand { }

        public class CommandHandler : IRequestHandler<Command>
        {
            public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
                => Unit.Task;
        }

        public class CommandWithCulture : ICommand { }

        public class CommandHandlerWithCulture : IRequestHandler<CommandWithCulture>
        {
            private readonly Action callback;
            
            public CommandHandlerWithCulture(Action callback)
                => this.callback = callback;

            public Task<Unit> Handle(CommandWithCulture request, CancellationToken cancellationToken)
            {
                callback();
                return Unit.Task;
            }
        }

        public class Notification : INotification { }

        public class NotificationHandler : INotificationHandler<Notification>
        {
            public Task Handle(Notification notification, CancellationToken cancellationToken)
                => Task.CompletedTask;
        }

        public class SyncNotificationHandler : NotificationHandler<Notification>
        {
            protected override void Handle(Notification notification) { }
        }

        public class Request : IRequest { }

        public class RequestHandler : AsyncRequestHandler<Request>
        {
            protected override Task Handle(Request request, CancellationToken cancellationToken)
                => Task.CompletedTask;
        }

        public class SyncRequest : IRequest { }

        public class SyncRequestHandler : RequestHandler<SyncRequest>
        {
            protected override void Handle(SyncRequest request) { }
        }
    }
}