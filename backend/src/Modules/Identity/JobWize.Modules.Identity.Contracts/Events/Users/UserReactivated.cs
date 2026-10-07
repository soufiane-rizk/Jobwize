using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Identity.Contracts.Events.Users;

public sealed record UserReactivated(Guid UserId, Guid ReactivatedByUserId) : IIntegrationEvent;
