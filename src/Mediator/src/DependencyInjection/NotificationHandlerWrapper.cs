// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Fusonic.Extensions.Mediator.DependencyInjection;

internal sealed class NotificationHandlerWrapper<T> : INotificationHandlerWrapper
    where T : INotification
{
    public async Task Handle(object notification, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<T>>();
        foreach (var handler in handlers)
        {
            await handler.Handle((T)notification, cancellationToken);
        }
    }
}