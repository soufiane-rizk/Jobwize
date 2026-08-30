using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobApplicationCvSubmitted(
    Guid SubmissionId,
    Guid JobApplicationId,
    Guid CandidateId,
    IReadOnlyList<Guid> FileIds) : IIntegrationEvent;
