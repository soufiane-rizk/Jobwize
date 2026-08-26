using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Identity.Contracts.Events.Users;

public sealed record UserCreated(Guid UserId, Guid CreatedByUserId) : IIntegrationEvent;
