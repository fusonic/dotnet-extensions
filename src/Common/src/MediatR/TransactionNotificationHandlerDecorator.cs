using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.Transactions;
using MediatR;

namespace Fusonic.Extensions.Common.MediatR
{
    /// <summary>
    /// Runs MediatR notifications within a transaction.
    ///
    /// Configuration with SimpleInjector:
    /// Container.RegisterDecorator(typeof(INotificationHandler{}), typeof(TransactionNotificationHandlerDecorator{}));
    /// </summary>
    public class TransactionNotificationHandlerDecorator<TNotification> : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> notificationHandler;
        private readonly ITransactionScopeHandler transactionScopeHandler;

        public TransactionNotificationHandlerDecorator(INotificationHandler<TNotification> notificationHandler, ITransactionScopeHandler transactionScopeHandler)
        {
            this.notificationHandler = notificationHandler;
            this.transactionScopeHandler = transactionScopeHandler;
        }

        public Task Handle(TNotification notification, CancellationToken cancellationToken)
            => transactionScopeHandler.RunInTransactionScope(() => notificationHandler.Handle(notification, cancellationToken));
    }
}