using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobApplicationReminderCreated(
    Guid ReminderId,
    Guid JobApplicationId,
    Guid CandidateId,
    ReminderKind Kind,
    DateTime DueAt) : IIntegrationEvent;
