// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Transactions;

namespace Fusonic.Extensions.Common.Transactions;

public static class TransactionScopeHandlerExtensions
{
    public static async Task<T> SuppressTransaction<T>(this ITransactionScopeHandler transactionScopeHandler, Func<Task<T>> action)
        => await transactionScopeHandler.RunInTransactionScope(action, TransactionScopeOption.Suppress);

    public static async Task SuppressTransaction(this ITransactionScopeHandler transactionScopeHandler, Func<Task> action)
        => await transactionScopeHandler.RunInTransactionScope(action, TransactionScopeOption.Suppress);
}