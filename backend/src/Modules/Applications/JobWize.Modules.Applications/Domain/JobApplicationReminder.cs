using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Shared.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplicationReminder : Entity
{
    public Guid JobApplicationId { get; private set; }
    public ReminderKind Kind { get; private set; }
    public Guid? CvSubmissionId { get; private set; }
    public Guid? InterviewId { get; private set; }
    public string Title { get; private set; } = default!;
    public DateTime DueAt { get; private set; }
    public string? Note { get; private set; }
    public ReminderState State { get; private set; }

    private JobApplicationReminder()
    {
    }

    internal static JobApplicationReminder Create(
        Guid applicationId,
        ReminderKind kind,
        Guid? cvSubmissionId,
        Guid? interviewId,
        string title,
        DateTime dueAt,
        string? note)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessRuleException(DomainErrors.ReminderTitleRequired);
        }

        if (dueAt == default)
        {
            throw new BusinessRuleException(DomainErrors.ReminderDueAtRequired);
        }

        if (!HasValidRelation(kind, cvSubmissionId, interviewId))
        {
            throw new BusinessRuleException(DomainErrors.ReminderRelationInvalid);
        }

        return new JobApplicationReminder
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            Kind = kind,
            CvSubmissionId = cvSubmissionId,
            InterviewId = interviewId,
            Title = title.Trim(),
            DueAt = ToUtc(dueAt),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            State = ReminderState.Open
        };
    }

    internal void ChangeState(ReminderState state)
    {
        if (State != ReminderState.Open)
        {
            throw new BusinessRuleException(DomainErrors.ReminderCannotChangeState);
        }

        if (state is not (ReminderState.Completed or ReminderState.Dismissed))
        {
            throw new BusinessRuleException(DomainErrors.ReminderStateInvalid);
        }

        State = state;
    }

    private static bool HasValidRelation(
        ReminderKind kind,
        Guid? cvSubmissionId,
        Guid? interviewId) =>
        kind switch
        {
            ReminderKind.CvSubmission => cvSubmissionId is not null && interviewId is null,
            ReminderKind.Interview => cvSubmissionId is null && interviewId is not null,
            ReminderKind.Custom => cvSubmissionId is null && interviewId is null,
            _ => false
        };

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
