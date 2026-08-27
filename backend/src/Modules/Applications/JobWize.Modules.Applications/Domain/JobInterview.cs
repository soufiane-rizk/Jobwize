using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobInterview : DomainModel
{
    public Guid JobApplicationId { get; private set; }
    public InterviewType Type { get; private set; }
    public InterviewState State { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int? DurationMinutes { get; private set; }
    public InterviewFormat Format { get; private set; }
    public string? Location { get; private set; }
    public string? PreparationNotes { get; private set; }
    private readonly List<JobInterviewParticipant> _participants = [];
    public IReadOnlyCollection<JobInterviewParticipant> Participants => _participants.AsReadOnly();

    private JobInterview()
    {
    }

    internal static JobInterview Schedule(
        Guid jobApplicationId,
        InterviewType type,
        DateTime scheduledAt,
        int? durationMinutes,
        InterviewFormat format,
        string? location,
        string? preparationNotes,
        IEnumerable<(string Name, string? RoleTitle)> participants)
    {
        if (scheduledAt == default)
        {
            throw new ArgumentException("An interview date is required.", nameof(scheduledAt));
        }

        if (durationMinutes is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        }

        DateTime scheduledAtUtc = ToUtc(scheduledAt);

        var interview = new JobInterview
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            Type = type,
            State = InterviewState.Scheduled,
            ScheduledAt = scheduledAtUtc,
            DurationMinutes = durationMinutes,
            Format = format,
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            PreparationNotes = string.IsNullOrWhiteSpace(preparationNotes) ? null : preparationNotes.Trim()
        };

        foreach ((string name, string? roleTitle) in participants)
        {
            interview._participants.Add(JobInterviewParticipant.Create(interview.Id, name, roleTitle));
        }

        return interview;
    }

    internal void Update(
        InterviewType type,
        DateTime scheduledAt,
        int? durationMinutes,
        InterviewFormat format,
        string? location,
        string? preparationNotes,
        IEnumerable<(string Name, string? RoleTitle)> participants)
    {
        if (State != InterviewState.Scheduled)
        {
            throw new InvalidOperationException("Only a scheduled interview can be updated.");
        }

        if (scheduledAt == default)
        {
            throw new ArgumentException("An interview date is required.", nameof(scheduledAt));
        }

        if (durationMinutes is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        }

        Type = type;
        ScheduledAt = ToUtc(scheduledAt);
        DurationMinutes = durationMinutes;
        Format = format;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        PreparationNotes = string.IsNullOrWhiteSpace(preparationNotes) ? null : preparationNotes.Trim();

        _participants.Clear();

        foreach ((string name, string? roleTitle) in participants)
        {
            _participants.Add(JobInterviewParticipant.Create(Id, name, roleTitle));
        }
    }

    internal void RecordResult(InterviewState state)
    {
        if (State != InterviewState.Scheduled)
        {
            throw new InvalidOperationException("Only a scheduled interview can have a result recorded.");
        }

        if (state == InterviewState.Scheduled)
        {
            throw new ArgumentException("Select a completed, cancelled, or postponed result.", nameof(state));
        }

        State = state;
    }

    internal JobInterview CreateRescheduledInterview(DateTime scheduledAt)
    {
        return Schedule(
            JobApplicationId,
            Type,
            scheduledAt,
            DurationMinutes,
            Format,
            Location,
            PreparationNotes,
            Participants.Select(participant => (participant.Name, participant.RoleTitle)));
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
