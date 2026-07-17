using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using static JobWize.ModuleOne.Features.CreateItem;

namespace JobWize.ModuleOne.Features
{
    internal static class OnItemCreated
    {
        internal sealed class NotificationHandler : INotificationHandler<ItemCreated>
        {
            private readonly INotificationStore _store;

            public NotificationHandler(INotificationStore store)
            {
                _store = store;
            }

            public Task HandleAsync(ItemCreated notification, CancellationToken cancellationToken)
            {
                _store.Published.Add(notification.Id);

                return Task.CompletedTask;
            }
        }
    }
}
