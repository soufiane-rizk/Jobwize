using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Dispatching
{
    public interface IDispatcher
    {
        public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
        public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);
        public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
