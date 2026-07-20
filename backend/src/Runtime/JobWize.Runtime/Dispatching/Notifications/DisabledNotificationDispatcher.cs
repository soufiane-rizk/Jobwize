using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching.Notifications
{
    public sealed class DisabledNotificationDispatcher : INotificationDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<INotification> notifications, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
