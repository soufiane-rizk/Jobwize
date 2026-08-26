using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Identity.Contracts.Events.Users;

public sealed record UserSuspended(Guid UserId, Guid SuspendedByUserId) : IIntegrationEvent;
