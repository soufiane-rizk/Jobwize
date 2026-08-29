using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyContactCreated(
    Guid CompanyId,
    Guid CompanyContactId,
    Guid CandidateId) : IIntegrationEvent;
