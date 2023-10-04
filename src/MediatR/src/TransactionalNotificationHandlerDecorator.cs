// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Transactions;
using MediatR;

namespace Fusonic.Extensions.MediatR;

/// <summary>
/// Runs MediatR notifications within a transaction.
///
/// Configuration with SimpleInjector:
/// Container.RegisterDecorator(typeof(INotificationHandler{}), typeof(TransactionNotificationHandlerDecorator{}));
/// </summary>
public class TransactionalNotificationHandlerDecorator<TNotification>(INotificationHandler<TNotification> notificationHandler, ITransactionScopeHandler transactionScopeHandler) : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
        => transactionScopeHandler.RunInTransactionScope(() => notificationHandler.Handle(notification, cancellationToken));
}
