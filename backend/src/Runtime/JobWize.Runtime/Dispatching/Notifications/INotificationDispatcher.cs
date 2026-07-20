using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching.Notifications
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(IReadOnlyCollection<INotification> notifications, CancellationToken cancellationToken = default);
    }
}
