using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Dispatching;
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
        private readonly INotificationContext _notificationContext;

        public MonolithExecutionModel(IServiceProvider serviceProvider, IModuleRegistry registry, INotificationContext notificationContext)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
            _notificationContext = notificationContext;
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

        public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            bool isRootPublication = _notificationContext.Begin();

            try
            {
                _notificationContext.Publish(notification);

                IModuleRuntime runtime = _registry.Resolve(notification.GetType());

                await runtime.PublishAsync(_serviceProvider, notification, ExecutionScope.Global, cancellationToken);

                if (isRootPublication)
                {
                    IReadOnlyCollection<INotification> notifications = _notificationContext.GetCurrentWave();

                }
            }
            finally
            {
                if (isRootPublication)
                {
                    _notificationContext.Complete();
                }
            }
        }
    }
}
