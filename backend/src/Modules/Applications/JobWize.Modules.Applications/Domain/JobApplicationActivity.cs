using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplicationActivity : Entity
{
    public Guid JobApplicationId { get; private set; }
    public ApplicationActivityType Type { get; private set; }
    public ApplicationStatus? Status { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Note { get; private set; }

    private JobApplicationActivity()
    {
    }

    internal static JobApplicationActivity CreateStatusChange(
        Guid applicationId,
        ApplicationStatus status,
        string? note)
    {
        return new JobApplicationActivity
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            Type = ApplicationActivityType.StatusChanged,
            Status = status,
            OccurredAt = DateTime.UtcNow,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };
    }

    internal static JobApplicationActivity CreateNote(Guid applicationId, string note)
    {
        return new JobApplicationActivity
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            Type = ApplicationActivityType.NoteAdded,
            OccurredAt = DateTime.UtcNow,
            Note = note.Trim()
        };
    }

    internal static JobApplicationActivity CreateInterviewScheduled(
        Guid applicationId,
        InterviewType interviewType,
        DateTime scheduledAt)
    {
        return new JobApplicationActivity
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            Type = ApplicationActivityType.InterviewScheduled,
            OccurredAt = DateTime.UtcNow,
            Note = $"{interviewType} interview scheduled."
        };
    }

    internal static JobApplicationActivity CreateInterviewResult(
        Guid applicationId,
        InterviewState state,
        string? note)
    {
        return new JobApplicationActivity
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            Type = ApplicationActivityType.InterviewResultRecorded,
            OccurredAt = DateTime.UtcNow,
            Note = string.IsNullOrWhiteSpace(note)
                ? state.ToString()
                : $"{state}. {note.Trim()}"
        };
    }
}
