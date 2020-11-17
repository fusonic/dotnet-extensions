// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Fusonic.Extensions.Common.Transactions
{
    public interface ITransactionScopeHandler
    {
        Task<T> RunInTransactionScope<T>(Func<Task<T>> action, TransactionScopeOption scopeOption = TransactionScopeOption.Required);
        Task RunInTransactionScope(Func<Task> action, TransactionScopeOption scopeOption = TransactionScopeOption.Required);
    }

    public class TransactionScopeHandler : ITransactionScopeHandler
    {
        /// <summary>
        ///   Default Options used for creating transactions with this util. Postgres isolation level is Read committed by default
        ///   Timeout is coming from the connection, not the transaction.
        ///   Also see http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx
        /// </summary>
        private static readonly TransactionOptions defaultTransactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.MaximumTimeout
        };

        public virtual async Task<T> RunInTransactionScope<T>(Func<Task<T>> action, TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            using var scope = CreateScope(scopeOption);
            var result = await action();
            scope.Complete();

            return result;
        }

        public virtual async Task RunInTransactionScope(Func<Task> action, TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            using var scope = CreateScope(scopeOption);
            await action();
            scope.Complete();
        }

        protected TransactionScope CreateScope(TransactionScopeOption scopeOption = TransactionScopeOption.Required)
            => new TransactionScope(scopeOption, defaultTransactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }
}