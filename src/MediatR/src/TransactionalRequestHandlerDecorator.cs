﻿// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Common.Transactions;
using MediatR;

namespace Fusonic.Extensions.MediatR;

/// <summary>
/// Runs mediatr requests within a transaction.
///
/// Configuration with SimpleInjector:
/// Container.RegisterDecorator(typeof(IRequestHandler{,}), typeof(TransactionalRequestHandlerDecorator{,}));
/// </summary>
public class TransactionalRequestHandlerDecorator<TCommand, TResult>(IRequestHandler<TCommand, TResult> requestHandler, ITransactionScopeHandler transactionScopeHandler) : IRequestHandler<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
    public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
        => transactionScopeHandler.RunInTransactionScope(() => requestHandler.Handle(request, cancellationToken));
}
