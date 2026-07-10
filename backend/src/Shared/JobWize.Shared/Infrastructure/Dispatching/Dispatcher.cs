using JobWize.Shared.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Dispatching
{
    public sealed class Dispatcher : IDispatcher
    {
        private readonly IMediator _mediator;
        private readonly IModuleDispatcher _moduleDispatcher;

        public Dispatcher(IMediator mediator, IModuleDispatcher moduleDispatcher)
        {
            _mediator = mediator;
            _moduleDispatcher = moduleDispatcher;
        }

        public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(query, cancellationToken);
        }

        public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(command, cancellationToken);
        }

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return _moduleDispatcher.SendAsync(query, cancellationToken);
        }
    }
}
