using JobWize.Shared.Contracts.Application.Events;
using JobWize.Shared.Runtime.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Dispatching
{
    public interface IIntegrationEventContext
    {
        bool IsActive { get; }

        bool Begin();

        void Collect(IIntegrationEvent integrationEvent);

        IReadOnlyCollection<IIntegrationEvent> Complete();

        void Abort();
    }
    internal sealed class IntegrationEventContext : IIntegrationEventContext
    {
        private bool _isActive;

        private readonly List<IIntegrationEvent> _events = [];

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

        public void Collect(IIntegrationEvent integrationEvent)
        {
            if (!_isActive)
            {
                throw new InvalidOperationException(
                    "Cannot collect integration events outside of an active transaction.");
            }

            _events.Add(integrationEvent);
        }

        public IReadOnlyCollection<IIntegrationEvent> Complete()
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
