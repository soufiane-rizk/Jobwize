using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyContactReviewed(
    Guid CompanyId,
    Guid CompanyContactId,
    Guid ReviewerId) : IIntegrationEvent;
