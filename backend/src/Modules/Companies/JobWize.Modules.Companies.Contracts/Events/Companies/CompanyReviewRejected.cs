using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Companies.Contracts.Events.Companies;

public sealed record CompanyReviewRejected(Guid CompanyId, Guid ReviewerId, string Reason) : IIntegrationEvent;
