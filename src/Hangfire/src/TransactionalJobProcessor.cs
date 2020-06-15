using System.Threading.Tasks;
using Fusonic.Extensions.Common.Transactions;
using Hangfire.Server;
using SimpleInjector;

namespace Fusonic.Extensions.Hangfire
{
    /// <summary>
    /// Runs [OutOfBand]-Jobs within a transaction.
    ///
    /// Configuration via SimpleInjector container:
    /// Container.RegisterOutOfBandDecorators(c => c.UseJobProcessor{TransactionalJobProcessor}());
    /// </summary>
    public sealed class TransactionalJobProcessor : JobProcessor
    {
        private readonly ITransactionScopeHandler transactionScopeHandler;

        public TransactionalJobProcessor(Container container, ITransactionScopeHandler transactionScopeHandler) : base(container)
        {
            this.transactionScopeHandler = transactionScopeHandler;
        }

        public override Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext)
            => transactionScopeHandler.RunInTransactionScope(() => base.ProcessAsync(context, performContext));
    }
}