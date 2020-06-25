﻿using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Common.Transactions;
using MediatR;

namespace Fusonic.Extensions.Common.MediatR
{
    /// <summary>
    /// Runs commands (MediatR requests) within a transaction.
    ///
    /// Configuration with SimpleInjector:
    /// Container.RegisterDecorator(typeof(IRequestHandler{,}), typeof(TransactionCommandHandlerDecorator{,}));
    /// </summary>
    public class TransactionalCommandHandlerDecorator<TCommand, TResult> : IRequestHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IRequestHandler<TCommand, TResult> requestHandler;
        private readonly ITransactionScopeHandler transactionScopeHandler;

        public TransactionalCommandHandlerDecorator(IRequestHandler<TCommand, TResult> requestHandler, ITransactionScopeHandler transactionScopeHandler)
        {
            this.requestHandler = requestHandler;
            this.transactionScopeHandler = transactionScopeHandler;
        }

        public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
            => transactionScopeHandler.RunInTransactionScope(() => requestHandler.Handle(request, cancellationToken));
    }
}