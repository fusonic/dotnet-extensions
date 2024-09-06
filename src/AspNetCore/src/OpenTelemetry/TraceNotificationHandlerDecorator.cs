// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

public class TraceNotificationHandlerDecorator<TNotification>(INotificationHandler<TNotification> notificationHandler)
    : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private static readonly string DisplayName = MediatorTracer.GetTypeName(typeof(TNotification));

    public async Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        await MediatorTracer.TraceRequest(
            notificationHandler.GetType(),
            DisplayName,
            kind: "Notification",
            async () =>
            {
                await notificationHandler.Handle(notification, cancellationToken);
                return Unit.Value;
            });
    }
}