using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Transactions
{
    public interface ITransactionManager
    {
        Task BeginAsync(CancellationToken cancellationToken = default);

        Task PersistChangesAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
