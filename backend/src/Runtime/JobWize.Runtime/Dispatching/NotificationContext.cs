using JobWize.Runtime.Contracts.Notifications;

using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching
{
    public interface INotificationContext
    {
        bool Begin();

        void Publish(INotification notification);

        IReadOnlyCollection<INotification> GetCurrentWave();

        bool MoveToNextWave();

        void Complete();
    }
    internal sealed class NotificationContext : INotificationContext
    {
        private int _depth;
        private bool _isExecutingWave;
        private readonly List<INotification> _currentWave = [];
        private readonly List<INotification> _nextWave = [];

        public bool Begin()
        {
            _depth++;

            return _depth == 1;
        }

        public void Publish(INotification notification)
        {
            if (_depth == 0)
            {
                throw new InvalidOperationException("Notifications can only be published during an active notification workflow.");
            }

            if (_isExecutingWave)
            {
                _nextWave.Add(notification);
            }
            else
            {
                _currentWave.Add(notification);
            }
        }

        public IReadOnlyCollection<INotification> GetCurrentWave()
        {
            _isExecutingWave = true;

            return _currentWave;
        }

        public bool MoveToNextWave()
        {
            _isExecutingWave = false;

            if (_nextWave.Count == 0)
            {
                return false;
            }

            _currentWave.Clear();
            _currentWave.AddRange(_nextWave);

            _nextWave.Clear();

            return true;
        }

        public void Complete()
        {
            _depth--;

            if (_depth != 0)
            {
                return;
            }

            _isExecutingWave = false;

            _currentWave.Clear();
            _nextWave.Clear();
        }
    }
}