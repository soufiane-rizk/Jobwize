using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Application.Events
{
    public interface IIntegrationEvent : INotification
    {
    }
}
