using JobWize.Shared.Contracts.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Events
{
    public interface IIntegrationEventHandler<TIntegrationEvent> 
        where TIntegrationEvent : IIntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken);
    }
}
