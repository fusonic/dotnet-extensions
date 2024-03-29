// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;

namespace Fusonic.Extensions.Hangfire.Tests;

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

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class OutOfBandNotificationHandlerWithoutAttribute : INotificationHandler<OutOfBandNotification>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public static event EventHandler<OutOfBandNotification>? Handled;

    public Task Handle(OutOfBandNotification notification, CancellationToken cancellationToken)
    {
        Handled?.Invoke(this, notification);
        return Task.CompletedTask;
    }
}

public class Request : IRequest { }

public class RequestHandler : AsyncRequestHandler<Request>
{
    protected override Task Handle(Request request, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
