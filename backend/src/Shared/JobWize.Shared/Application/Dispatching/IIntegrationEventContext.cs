using JobWize.Shared.Contracts.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Dispatching
{
    public interface IIntegrationEventContext
    {
        bool IsActive { get; }

        bool Begin();

        void Collect(IIntegrationEvent integrationEvent);

        IReadOnlyCollection<IIntegrationEvent> Complete();

        void Abort();
    }
}
