using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching.Notifications
{
    public sealed class ImmediateNotificationDispatcher : INotificationDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleRegistry _registry;

        public ImmediateNotificationDispatcher(IServiceProvider serviceProvider, IModuleRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public async Task DispatchAsync(IReadOnlyCollection<INotification> notifications, CancellationToken cancellationToken = default)
        {
            foreach (INotification notification in notifications)
            {
                IModuleRuntime runtime = _registry.Resolve(notification.GetType());

                await runtime.PublishAsync(_serviceProvider, notification, cancellationToken);
            }
        }
    }
}
