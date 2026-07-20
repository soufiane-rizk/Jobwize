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

        public IReadOnlyCollection<INotification>? Notifications { get; private set; }

        public Task DispatchAsync(IReadOnlyCollection<INotification> notifications, CancellationToken cancellationToken = default)
        {
            DispatchCalled = true;
            Notifications = notifications;

            return Task.CompletedTask;
        }
    }
}
