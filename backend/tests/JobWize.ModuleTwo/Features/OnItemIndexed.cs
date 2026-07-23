using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleTwo.Features
{
    internal sealed class ItemIndexedHandler : INotificationHandler<ItemIndexed>
    {
        private readonly IModuleTwoNotificationStore _store;

        public ItemIndexedHandler(IModuleTwoNotificationStore store)
        {
            _store = store;
        }

        public Task HandleAsync(ItemIndexed notification, CancellationToken cancellationToken)
        {
            _store.ItemIndexedReceived.Add(notification.Id);

            return Task.CompletedTask;
        }
    }
}
