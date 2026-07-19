using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Dispatching
{
    public interface IDispatcher
    {
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);
        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
    }

}
