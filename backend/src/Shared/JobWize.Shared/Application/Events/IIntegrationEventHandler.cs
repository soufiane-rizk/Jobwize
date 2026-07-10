using JobWize.Shared.Contracts.Application.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Events
{
    public interface IIntegrationEventHandler<TIntegrationEvent> 
        : INotificationHandler<TIntegrationEvent> 
        where TIntegrationEvent : IIntegrationEvent
    {
    }
}
