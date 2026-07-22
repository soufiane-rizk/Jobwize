using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Dispatching;
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
            private readonly IDispatcher _dispatcher;


            public Handler(IItemRepository repository, IDispatcher dispatcher)
            {
                _repository = repository;
                _dispatcher = dispatcher;
            }

            public async Task<Result<Guid>> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                Guid id = _repository.Create(request.Name);

                await _dispatcher.PublishAsync(new ItemCreated(id), cancellationToken);

                return Result<Guid>.Success(id);
            }
        }
    }
}
