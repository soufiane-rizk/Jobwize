using JobWize.Shared.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Dispatching
{
    public class IntegrationEventContext : IIntegrationEventContext
    {
        private bool _isActive;

        private readonly List<IIntegrationEvent> _events = [];

        public bool IsActive => throw new NotImplementedException();

        public bool Begin()
        {
            throw new NotImplementedException();
        }

        public void Collect(IIntegrationEvent integrationEvent)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<IIntegrationEvent> Complete()
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
