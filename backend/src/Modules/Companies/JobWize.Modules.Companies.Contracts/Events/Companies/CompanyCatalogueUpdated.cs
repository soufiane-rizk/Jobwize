using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyCatalogueUpdated(
    Guid CompanyId,
    Guid UpdatedByUserId) : IIntegrationEvent;
