// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector;

namespace Fusonic.Extensions.Mediator.SimpleInjector;

internal sealed class NotificationHandlerWrapper<T> : INotificationHandlerWrapper
    where T : INotification
{
    public async Task Handle(object notification, Container container, CancellationToken cancellationToken)
    {
        var handlers = container.GetAllInstances<INotificationHandler<T>>();
        foreach (var handler in handlers)
        {
            await handler.Handle((T)notification, cancellationToken);
        }
    }
}