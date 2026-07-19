using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeNotificationContext : INotificationContext
    {
        public bool BeginCalled { get; private set; }

        public bool CollectCalled { get; private set; }

        public bool CompleteCalled { get; private set; }

        public bool BeginResult { get; set; } = true;

        private readonly List<INotification> _notifications = [];

        public IReadOnlyList<INotification> Notifications => _notifications;

        public bool IsActive => BeginCalled && !CompleteCalled;

        public bool Begin()
        {
            BeginCalled = true;
            return BeginResult;
        }

        public void Collect(INotification notification)
        {
            CollectCalled = true;
            _notifications.Add(notification);
        }

        public IReadOnlyCollection<INotification> Complete()
        {
            CompleteCalled = true;
            return Notifications;
        }

        public void Abort()
        {
            _notifications.Clear();
        }
    }
}
