using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleTwo.Features
{
    internal sealed class ItemCreatedHandler : INotificationHandler<ItemCreated>
    {
        private readonly IModuleTwoNotificationStore _store;

        public ItemCreatedHandler(IModuleTwoNotificationStore store)
        {
            _store = store;
        }

        public Task HandleAsync(ItemCreated notification, CancellationToken cancellationToken)
        {
            _store.ItemCreatedReceived.Add(notification.Id);

            return Task.CompletedTask;
        }
    }
}
