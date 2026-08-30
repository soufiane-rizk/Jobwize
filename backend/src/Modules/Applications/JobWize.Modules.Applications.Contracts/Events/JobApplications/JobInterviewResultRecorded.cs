using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobInterviewResultRecorded(
    Guid InterviewId,
    Guid JobApplicationId,
    Guid CandidateId,
    InterviewState State) : IIntegrationEvent;
