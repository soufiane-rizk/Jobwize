using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyCreated(
    Guid CompanyId,
    Guid CandidateId) : IIntegrationEvent;
