using JobWize.Runtime.Contracts.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Transactions
{
    internal sealed class RecordingTransactionContext : ITransactionContext
    {
        public string Name { get; }

        public List<string> Calls { get; } = [];

        public RecordingTransactionContext(string name)
        {
            Name = name;
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add($"{Name}.Begin");

            return Task.CompletedTask;
        }

        public Task PersistChangesAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add($"{Name}.Persist");

            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add($"{Name}.Commit");

            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            Calls.Add($"{Name}.Rollback");

            return Task.CompletedTask;
        }
    }
}
