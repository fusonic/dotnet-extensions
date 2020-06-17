using System;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.MediatR;
using MediatR;

namespace Fusonic.Extensions.Hangfire.Tests
{
    public class OutOfBandCommand : ICommand { }

    [OutOfBand]
    public class OutOfBandCommandHandler : IRequestHandler<OutOfBandCommand>
    {
        public static event EventHandler<OutOfBandCommand>? Handled;

        public Task<Unit> Handle(OutOfBandCommand request, CancellationToken cancellationToken)
        {
            Handled?.Invoke(this, request);
            return Unit.Task;
        }
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

    public class OutOfBandNotification : INotification { }

    [OutOfBand]
    public class OutOfBandNotificationHandler : INotificationHandler<OutOfBandNotification>
    {
        public static event EventHandler<OutOfBandNotification>? Handled;

        public Task Handle(OutOfBandNotification notification, CancellationToken cancellationToken)
        {
            Handled?.Invoke(this, notification);
            return Task.CompletedTask;
        }
    }

    public class OutOfBandNotificationHandlerWithoutAttribute : INotificationHandler<OutOfBandNotification>
    {
        public static event EventHandler<OutOfBandNotification>? Handled;

        public Task Handle(OutOfBandNotification notification, CancellationToken cancellationToken)
        {
            Handled?.Invoke(this, notification);
            return Task.CompletedTask;
        }
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