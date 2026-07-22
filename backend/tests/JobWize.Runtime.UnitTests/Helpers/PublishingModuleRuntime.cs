using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class PublishingModuleRuntime : FakeModuleRuntime
    {
        private readonly INotificationContext _notificationContext;
        private readonly INotification _notification;

        public PublishingModuleRuntime(INotificationContext notificationContext, INotification notification)
        {
            _notificationContext = notificationContext;
            _notification = notification;
        }

        public override Task PublishAsync(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken = default)
        {
            _notificationContext.Publish(_notification);

            return base.PublishAsync(serviceProvider, notification, cancellationToken);
        }
    }
}
