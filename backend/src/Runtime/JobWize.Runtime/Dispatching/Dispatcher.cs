using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using System;
using System.Collections.Generic;
using System.Text;


namespace JobWize.Runtime.Dispatching
{
    public sealed class Dispatcher : IDispatcher
    {
        private readonly IExecutionModel _executionModel;

        public Dispatcher(IExecutionModel executionModel)
        {
            _executionModel = executionModel;
        }

        public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            await _executionModel.PublishAsync(notification, cancellationToken);
            return;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return _executionModel.SendAsync<TResponse>(request, cancellationToken);
        }   

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return _executionModel.SendAsync<TResponse>(query, cancellationToken);
        }
    }
}
