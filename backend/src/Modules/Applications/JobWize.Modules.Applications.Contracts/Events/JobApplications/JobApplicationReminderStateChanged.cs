using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Applications.Contracts.Events.JobApplications;

public sealed record JobApplicationReminderStateChanged(
    Guid ReminderId,
    Guid JobApplicationId,
    Guid CandidateId,
    ReminderState State) : IIntegrationEvent;
