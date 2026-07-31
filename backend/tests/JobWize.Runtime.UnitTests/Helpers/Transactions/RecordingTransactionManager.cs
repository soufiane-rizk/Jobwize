using JobWize.Runtime.Contracts.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Transactions
{
    public sealed class RecordingTransactionManager : ITransactionManager
    {
        public List<string> Calls { get; } = [];

        public Task BeginAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add("Begin");
            return Task.CompletedTask;
        }

        public Task PersistChangesAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add("Persist");
            return Task.CompletedTask;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add("Commit");
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add("Rollback");
            return Task.CompletedTask;
        }
    }
}
