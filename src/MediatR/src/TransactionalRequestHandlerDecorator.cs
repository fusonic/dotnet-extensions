using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.Transactions;
using MediatR;

namespace Fusonic.Extensions.MediatR
{
    /// <summary>
    /// Runs mediatr requests within a transaction.
    ///
    /// Configuration with SimpleInjector:
    /// Container.RegisterDecorator(typeof(IRequestHandler{,}), typeof(TransactionalRequestHandlerDecorator{,}));
    /// </summary>
    public class TransactionalRequestHandlerDecorator<TCommand, TResult> : IRequestHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IRequestHandler<TCommand, TResult> requestHandler;
        private readonly ITransactionScopeHandler transactionScopeHandler;

        public TransactionalRequestHandlerDecorator(IRequestHandler<TCommand, TResult> requestHandler, ITransactionScopeHandler transactionScopeHandler)
        {
            this.requestHandler = requestHandler;
            this.transactionScopeHandler = transactionScopeHandler;
        }

        public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
            => transactionScopeHandler.RunInTransactionScope(() => requestHandler.Handle(request, cancellationToken));
    }
}