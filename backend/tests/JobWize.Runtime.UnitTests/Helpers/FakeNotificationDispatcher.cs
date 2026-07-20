using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Dispatching.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeNotificationDispatcher : INotificationDispatcher
    {
        public bool DispatchCalled { get; private set; }

        private readonly List<INotification> _notifications = [];

        public IReadOnlyCollection<INotification> Notifications => _notifications;

        public Task DispatchAsync(IReadOnlyCollection<INotification> notifications, CancellationToken cancellationToken = default)
        {
            DispatchCalled = true;

            _notifications.Clear();
            _notifications.AddRange(notifications);

            return Task.CompletedTask;
        }
    }
}
