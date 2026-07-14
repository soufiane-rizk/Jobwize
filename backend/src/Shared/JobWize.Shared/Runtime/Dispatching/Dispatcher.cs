using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using JobWize.Shared.Runtime.Dispatching;
using JobWize.Shared.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace JobWize.Shared.Infrastructure.Dispatching
{
    public interface IDispatcher
    {
        public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
        public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);
        public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }

    public sealed class Dispatcher : IDispatcher
    {
        //private readonly IMediator _mediator;
        private readonly IModuleDispatcher _moduleDispatcher;
        private readonly IIntegrationEventContext _integrationEventContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleRegistry _registry;

        public Dispatcher(IModuleDispatcher moduleDispatcher, IIntegrationEventContext integrationEventContext, IServiceProvider serviceProvider, IModuleRegistry registry)
        {
            //_mediator = mediator;
            _moduleDispatcher = moduleDispatcher;
            _integrationEventContext = integrationEventContext;
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            bool isRootPublication = _integrationEventContext.Begin();

            _integrationEventContext.Collect(integrationEvent);

            //await _mediator.Publish(integrationEvent, cancellationToken);

            if (isRootPublication)
            {
                IReadOnlyCollection<IIntegrationEvent> integrationEvents =
                    _integrationEventContext.Complete();

                // TODO:
                // Persist integrationEvents to the Outbox.
            }
        }

        public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            ModuleRuntime runtime = _registry.Resolve(query.GetType());

            return runtime.SendAsync(_serviceProvider, query, cancellationToken);
        }

        public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            ModuleRuntime runtime = _registry.Resolve(command.GetType());

            return runtime.SendAsync(_serviceProvider, command, cancellationToken);
        }

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return _moduleDispatcher.SendAsync(query, cancellationToken);
        }
    }
}
