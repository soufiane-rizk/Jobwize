using FluentValidation;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal static class ValidationCommand
    {
        internal sealed record Command(string Name, int Age) : ICommand<Guid>;

        internal sealed class Handler : ICommandHandler<Command, Guid>
        {
            public Task<Result<Guid>> HandleAsync(Command command, CancellationToken cancellationToken)
            {
                return Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));
            }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.Age)
                    .GreaterThanOrEqualTo(18);
            }
        }

        internal sealed class SecondValidator : AbstractValidator<Command>
        {
            public SecondValidator()
            {
                RuleFor(x => x.Name)
                    .MinimumLength(3);
            }
        }
    }
}
