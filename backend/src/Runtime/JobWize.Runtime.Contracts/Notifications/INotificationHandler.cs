using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Notifications
{
    public interface INotificationHandler<TNotification> where TNotification : INotification
    {
        Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
    }
}
