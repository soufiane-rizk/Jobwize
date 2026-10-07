using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.Extensions.Logging;

namespace JobWize.Shared.Runtime.Behaviors
{
    public sealed class ExceptionHandlingBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, Result<TResult>>
        where TCommand : IUseCase<TResult>
    {
        private readonly ILogger<ExceptionHandlingBehavior<TCommand, TResult>> _logger;

        public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TCommand, TResult>> logger)
        {
            _logger = logger;
        }

        public async Task<Result<TResult>> HandleAsync(ExecutionContext<TCommand, Result<TResult>> context, RequestExecutionDelegate<Result<TResult>> next)
        {
            try
            {
                return await next();
            }
            catch (BusinessRuleException exception)
            {
                _logger.LogInformation(
                    "Business rule prevented {Command}: {ErrorCode}.",
                    typeof(TCommand).Name,
                    exception.Error.Code);

                return Result<TResult>.Failure(exception.Error);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while executing {Command}.",
                    typeof(TCommand).Name);

                return Result<TResult>.Failure(SharedErrors.Unexpected);
            }
        }
    }
}
