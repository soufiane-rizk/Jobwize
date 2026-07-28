using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Contracts.Transactions;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Behaviors
{
    public sealed class TransactionBehavior<TCommand, TResult>  : IPipelineBehavior<TCommand, Result<TResult>>
        where TCommand : ICommand<TResult>
    {
        private readonly ITransactionManager _transactionManager;

        public TransactionBehavior(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        public async Task<Result<TResult>> HandleAsync(ExecutionContext<TCommand, Result<TResult>> context, RequestExecutionDelegate<Result<TResult>> next)
        {
            try
            {
                await _transactionManager.BeginAsync();

                var result = await next();

                if (result.IsSuccess)
                {
                    await _transactionManager.PersistChangesAsync();
                    await _transactionManager.CommitAsync();
                }
                else
                {
                    await _transactionManager.RollbackAsync();
                }

                return result;
            }
            catch
            {
                await _transactionManager.RollbackAsync();
                throw;
            }
        }


    }
}


