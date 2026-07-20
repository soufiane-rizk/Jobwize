using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    public interface IExecutionModel
    {
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);

        Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
    }
}
