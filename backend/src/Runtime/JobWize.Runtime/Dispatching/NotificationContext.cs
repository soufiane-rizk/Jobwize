using JobWize.Runtime.Contracts.Notifications;

using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching
{
    public interface INotificationContext
    {
        bool IsActive { get; }

        bool Begin();

        void Collect(INotification Notification);

        IReadOnlyCollection<INotification> Complete();

        void Abort();
    }
    internal sealed class NotificationContext : INotificationContext
    {
        private bool _isActive;

        private readonly List<INotification> _events = [];

        public bool IsActive => _isActive;

        public bool Begin()
        {
            if (_isActive)
            {
                return false;
            }

            _events.Clear();
            _isActive = true;

            return true;
        }

        public void Collect(INotification Notification)
        {
            if (!_isActive)
            {
                throw new InvalidOperationException(
                    "Cannot collect integration events outside of an active transaction.");
            }

            _events.Add(Notification);
        }

        public IReadOnlyCollection<INotification> Complete()
        {
            if (!_isActive)
            {
                throw new InvalidOperationException(
                    "No active transaction.");
            }

            var events = _events.ToArray();

            _events.Clear();
            _isActive = false;

            return events;
        }

        public void Abort()
        {
            _events.Clear();
            _isActive = false;
        }
    }
}