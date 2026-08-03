using JobWize.Shared.Contracts.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Events.Authentication
{
    public sealed record UserLoggedIn(Guid UserId) : IIntegrationEvent;

}
