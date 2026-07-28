using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Transactions
{
    public interface ITransactionContext
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task PersistChangesAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
