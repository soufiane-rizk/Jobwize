using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    internal sealed class MonolithExecutionModel : IExecutionModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleRegistry _registry;

        public MonolithExecutionModel(IServiceProvider serviceProvider, IModuleRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            IModuleRuntime runtime = _registry.Resolve(request.GetType());

            return runtime.SendAsync(_serviceProvider, request, cancellationToken);
        }

        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            IModuleRuntime runtime = _registry.Resolve(query.GetType());

            return runtime.SendAsync(_serviceProvider, query, cancellationToken);
        }

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
