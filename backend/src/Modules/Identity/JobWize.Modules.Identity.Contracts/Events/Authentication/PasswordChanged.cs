using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Identity.Contracts.Events.Authentication;

public sealed record PasswordChanged(Guid UserId) : IIntegrationEvent;
