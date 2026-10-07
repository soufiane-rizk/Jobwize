using FluentValidation;
using FluentValidation.Results;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Runtime.Contracts;

namespace JobWize.Shared.Runtime.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, Result<TResult>>
        where TRequest : ICommand<TResult>
    {
        private readonly IReadOnlyList<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators.ToList();
        }

        public async Task<Result<TResult>> HandleAsync(ExecutionContext<TRequest, Result<TResult>> context, RequestExecutionDelegate<Result<TResult>> next)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var validationTasks = _validators.Select(v =>
            {
                // ValidationContext is mutable and should not be shared across
                // concurrent validator executions.
                ValidationContext<TRequest> validationContext =
                    new(context.Request);

                return v.ValidateAsync(validationContext, context.CancellationToken);
            });

            ValidationResult[] results = await Task.WhenAll(validationTasks);

            List<ValidationFailure> failures = results
                .SelectMany(x => x.Errors)
                .ToList();

            if (failures.Count == 0)
            {
                return await next();
            }

            return Result<TResult>.Failure(CreateValidationError(failures));
        }

        private static Error CreateValidationError(IReadOnlyCollection<ValidationFailure> failures)
        {
            return new(
                "Runtime.ValidationFailed",
                "One or more validation errors occurred.",
                ErrorType.Validation,
                failures.Select(x => new ErrorDetail(x.PropertyName, x.ErrorMessage)).ToArray());
        }
    }
}
