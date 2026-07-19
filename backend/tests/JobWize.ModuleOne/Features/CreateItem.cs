using JobWize.Runtime.Contracts.Notifications;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Features
{
    public static class CreateItem
    {
        public sealed record Command(string Name) : ICommand<Guid>;

        internal sealed class Handler : ICommandHandler<Command, Guid>
        {
            private readonly IItemRepository _repository;

            public Handler(IItemRepository repository)
            {
                _repository = repository;
            }

            public Task<Result<Guid>> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                Guid id = _repository.Create(request.Name);

                return Task.FromResult(Result<Guid>.Success(id));
            }
        }
    }
}
