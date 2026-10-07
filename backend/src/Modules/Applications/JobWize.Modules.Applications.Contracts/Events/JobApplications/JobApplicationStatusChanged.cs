using JobWize.Shared.Contracts.Application.Events;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobApplicationStatusChanged(
    Guid JobApplicationId,
    Guid CandidateId,
    ApplicationStatus Status) : IIntegrationEvent;
