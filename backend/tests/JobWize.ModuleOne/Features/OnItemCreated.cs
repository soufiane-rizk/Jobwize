using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Dispatching;

namespace JobWize.ModuleOne.Features
{
    internal sealed class ItemCreatedHandler : INotificationHandler<ItemCreated>
    {
        private readonly IModuleOneNotificationStore _store;
        private readonly IDispatcher _dispatcher;

        public ItemCreatedHandler(IModuleOneNotificationStore store, IDispatcher dispatcher)
        {
            _store = store;
            _dispatcher = dispatcher;
        }

        public async Task HandleAsync(ItemCreated notification, CancellationToken cancellationToken)
        {
            _store.Published.Add(notification.Id);

            await _dispatcher.PublishAsync(new ItemIndexed(notification.Id), cancellationToken);
        }
    }  
}
