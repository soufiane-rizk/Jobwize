using JobWize.Runtime.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Persistence
{
    public abstract class ModuleDbContext : DbContext, ITransactionContext
    {
        private IDbContextTransaction? _transaction;

        protected ModuleDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return BeginTransactionInternalAsync(cancellationToken);
        }

        private async Task BeginTransactionInternalAsync(CancellationToken cancellationToken)
        {
            _transaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            return CommitInternalAsync(cancellationToken);
        }

        private async Task CommitInternalAsync(CancellationToken cancellationToken)
        {
            if (_transaction is null)
            {
                return;
            }

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            return RollbackInternalAsync(cancellationToken);
        }

        private async Task RollbackInternalAsync(CancellationToken cancellationToken)
        {
            if (_transaction is null)
            {
                return;
            }

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public Task PersistChangesAsync(CancellationToken cancellationToken)
        {
            return SaveChangesAsync(cancellationToken);
        }
    }
}
