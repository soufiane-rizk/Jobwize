using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyPromotedToShared(Guid CompanyId, Guid ReviewerId) : IIntegrationEvent;
