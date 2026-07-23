using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Contracts
{
    public record ItemCreated(Guid Id) : INotification;

}
