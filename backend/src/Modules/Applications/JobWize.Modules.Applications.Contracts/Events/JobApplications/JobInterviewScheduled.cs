using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobInterviewScheduled(Guid InterviewId, Guid JobApplicationId, Guid CandidateId) : IIntegrationEvent;
