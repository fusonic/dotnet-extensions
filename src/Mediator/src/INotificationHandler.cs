// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled</typeparam>
public interface INotificationHandler<in TNotification> : INotificationHandlerBase
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification
    /// </summary>
    /// <param name="notification">The notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TNotification notification, CancellationToken cancellationToken);

    async Task INotificationHandlerBase.Handle(object notification, CancellationToken cancellationToken)
        => await Handle((TNotification)notification, cancellationToken);
}

/// <summary>
/// Base Notification Handler that can be casted to, without knowing the request type
/// </summary>
public interface INotificationHandlerBase
{
    Task Handle(object notification, CancellationToken cancellationToken);
}