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
        private readonly IModuleDispatcher _moduleDispatcher;
        private readonly INotificationContext _notificationContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleRegistry _registry;

        public Dispatcher(IModuleDispatcher moduleDispatcher, INotificationContext notificationContext, IServiceProvider serviceProvider, IModuleRegistry registry)
        {
            //_mediator = mediator;
            _moduleDispatcher = moduleDispatcher;
            _notificationContext = notificationContext;
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            bool isRootPublication = _notificationContext.Begin();

            _notificationContext.Collect(notification);

            IModuleRuntime runtime = _registry.Resolve(notification.GetType());

            await runtime.PublishAsync(_serviceProvider, notification, cancellationToken);

            if (isRootPublication)
            {
                IReadOnlyCollection<INotification> notifications = _notificationContext.Complete();

                // TODO:
                // Persist integrationEvents to the Outbox.
            }
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            IModuleRuntime runtime = _registry.Resolve(request.GetType());

            return runtime.SendAsync(_serviceProvider, request, cancellationToken);
        }   

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return _moduleDispatcher.SendAsync(query, cancellationToken);
        }
    }
}
