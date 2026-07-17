using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Runtime.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Features
{
    public static class CreateItem
    {
        public sealed record Command(string Name) : ICommand<Result<Guid>>;

        internal sealed class Handler : ICommandHandler<Command, Result<Guid>>
        {
            public Task<Result<Guid>> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));
            }
        }
    }
}
