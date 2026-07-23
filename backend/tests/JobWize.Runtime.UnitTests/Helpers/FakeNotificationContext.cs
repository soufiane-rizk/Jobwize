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

        public bool PublishCalled { get; private set; }

        public bool GetCurrentWaveCalled { get; private set; }

        public bool MoveToNextWaveCalled { get; private set; }

        public bool CompleteCalled { get; private set; }

        public bool BeginResult { get; set; } = true;

        private bool _isExecutingWave;

        private readonly List<INotification> _currentWave = [];

        private readonly List<INotification> _nextWave = [];

        public IReadOnlyList<INotification> CurrentWave => _currentWave;

        public IReadOnlyList<INotification> NextWave => _nextWave;
        public INotification? PublishedNotification { get; private set; }

        public bool Begin()
        {
            BeginCalled = true;

            return BeginResult;
        }

        public void Publish(INotification notification)
        {
            PublishCalled = true;
            PublishedNotification = notification;

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
            GetCurrentWaveCalled = true;

            _isExecutingWave = true;

            return _currentWave;
        }

        public bool MoveToNextWave()
        {
            MoveToNextWaveCalled = true;

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
            CompleteCalled = true;

            _isExecutingWave = false;

            _currentWave.Clear();
            _nextWave.Clear();
        }
    }
}
