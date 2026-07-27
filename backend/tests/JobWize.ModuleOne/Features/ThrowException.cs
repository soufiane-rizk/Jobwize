using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Features
{
    public static class ThrowException
    {
        public sealed record Command() : ICommand<Guid>;

        internal sealed class Handler : ICommandHandler<Command, Guid>
        {
            public Task<Result<Guid>> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Boom!");
            }
        }
    }
}
