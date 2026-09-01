using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class ExceptionHandlingCommandBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, Result<TResult>>
        where TCommand : ICommand<TResult>
    {
        public async Task<Result<TResult>> HandleAsync(
            ExecutionContext<TCommand, Result<TResult>> context,
            RequestExecutionDelegate<Result<TResult>> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                return Result<TResult>.Failure(
                    new Error(
                        "Test.Exception",
                        ex.Message,
                        ErrorType.Conflict));
            }
        }
    }
}
