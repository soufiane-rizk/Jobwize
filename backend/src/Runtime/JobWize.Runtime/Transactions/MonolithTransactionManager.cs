using JobWize.Runtime.Contracts.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Transactions
{
    internal class MonolithTransactionManager : ITransactionManager
    {
        private readonly IEnumerable<ITransactionContext> _transactionContexts;

        public MonolithTransactionManager(IEnumerable<ITransactionContext> transactionContexts)
        {
            _transactionContexts = transactionContexts;
        }

        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            foreach (var transactionContext in _transactionContexts)
            {
                await transactionContext.BeginTransactionAsync(cancellationToken);
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var transactionContext in _transactionContexts)
            {
                await transactionContext.CommitTransactionAsync(cancellationToken);
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            foreach (var transactionContext in _transactionContexts)
            {
                await transactionContext.RollbackTransactionAsync(cancellationToken);
            }
        }

        public async Task PersistChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var transactionContext in _transactionContexts)
            {
                await transactionContext.PersistChangesAsync(cancellationToken);
            }
        }
    }
}
