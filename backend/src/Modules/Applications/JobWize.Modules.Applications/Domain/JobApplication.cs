using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplication : DomainModel
{
    public Guid CandidateId { get; private set; }
    public string? LegacyCompanyName { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? CompanyLocationId { get; private set; }
    public string? RoleTitle { get; private set; }
    public ApplicationKind Kind { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateOnly? AppliedOn { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? Notes { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    private readonly List<JobApplicationActivity> _activities = [];
    public IReadOnlyCollection<JobApplicationActivity> Activities => _activities.AsReadOnly();
    private readonly List<JobInterview> _interviews = [];
    public IReadOnlyCollection<JobInterview> Interviews => _interviews.AsReadOnly();
    private readonly List<JobApplicationCvSubmission> _cvSubmissions = [];
    public IReadOnlyCollection<JobApplicationCvSubmission> CvSubmissions => _cvSubmissions.AsReadOnly();
    public IReadOnlyList<ApplicationStatus> AllowedNextStatuses => GetAllowedNextStatuses();

    private JobApplication()
    {
    }

    public static JobApplication Create(
        Guid candidateId,
        Guid companyId,
        Guid? companyLocationId,
        string? roleTitle,
        ApplicationKind kind,
        ApplicationStatus status,
        DateOnly? appliedOn,
        string? sourceUrl,
        string? notes)
    {
        if (RequiresAppliedOn(status) && appliedOn is null)
        {
            throw new ArgumentException(
                "Applied on is required when the application has been sent.",
                nameof(appliedOn));
        }

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            LegacyCompanyName = null,
            CompanyId = companyId,
            CompanyLocationId = companyLocationId,
            RoleTitle = string.IsNullOrWhiteSpace(roleTitle) ? null : roleTitle.Trim(),
            Kind = kind,
            Status = status,
            AppliedOn = appliedOn,
            LastActivityAt = DateTime.UtcNow,
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        application._activities.Add(JobApplicationActivity.CreateStatusChange(application.Id, status, null));

        return application;
    }

    public void ChangeStatus(
        ApplicationStatus status,
        DateOnly? appliedOn,
        string? note)
    {
        if (status == Status)
        {
            throw new ArgumentException("The new status must be different from the current status.", nameof(status));
        }

        if (!AllowedNextStatuses.Contains(status))
        {
            throw new InvalidOperationException(
                $"Cannot change an application from {Status} to {status}.");
        }

        DateOnly? effectiveAppliedOn = appliedOn ?? AppliedOn;

        if (RequiresAppliedOn(status) && effectiveAppliedOn is null)
        {
            throw new ArgumentException(
                "Applied on is required once an application has been sent.",
                nameof(appliedOn));
        }

        Status = status;
        AppliedOn = effectiveAppliedOn;

        AddActivity(JobApplicationActivity.CreateStatusChange(Id, status, note));
    }

    public void AddNote(string note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(note);

        AddActivity(JobApplicationActivity.CreateNote(Id, note));
    }

    public JobApplicationCvSubmission RecordCvSubmission(
        DateTime sentAt,
        CvSubmissionMethod method,
        string? notes,
        (Guid? Id, Guid? LocationId, string? Name, string? RoleTitle, string? Email, string? PhoneNumber) contact,
        IEnumerable<(Guid FileId, string FileName, string ContentType, long SizeBytes)> documents)
    {
        DateTime normalizedSentAt = sentAt.Kind switch
        {
            DateTimeKind.Utc => sentAt,
            DateTimeKind.Local => sentAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(sentAt, DateTimeKind.Utc)
        };

        (Guid FileId, string FileName, string ContentType, long SizeBytes)[] documentSnapshots = documents.ToArray();

        if (documentSnapshots.Length == 0)
        {
            throw new ArgumentException("At least one document is required.", nameof(documents));
        }

        if (documentSnapshots.Select(document => document.FileId).Distinct().Count() != documentSnapshots.Length)
        {
            throw new ArgumentException("A document can only be submitted once.", nameof(documents));
        }

        JobApplicationCvSubmission submission = JobApplicationCvSubmission.Create(
            Id,
            normalizedSentAt,
            method,
            notes,
            contact,
            documentSnapshots);

        _cvSubmissions.Add(submission);

        if (Status is ApplicationStatus.Draft or ApplicationStatus.Planned)
        {
            Status = ApplicationStatus.Applied;
            AppliedOn = DateOnly.FromDateTime(normalizedSentAt);
            AddActivity(JobApplicationActivity.CreateStatusChange(
                Id,
                ApplicationStatus.Applied,
                "Automatically marked as applied when the CV submission was recorded."));
        }

        AddActivity(JobApplicationActivity.CreateCvSubmitted(
            Id,
            method,
            contact.Name));

        return submission;
    }

    public JobInterview ScheduleInterview(
        InterviewType interviewType,
        DateTime scheduledAt,
        int? durationMinutes,
        InterviewFormat format,
        string? location,
        string? preparationNotes,
        IEnumerable<InterviewParticipantSnapshot> participants)
    {
        JobInterview interview = JobInterview.Schedule(
            Id,
            interviewType,
            scheduledAt,
            durationMinutes,
            format,
            location,
            preparationNotes,
            participants);

        _interviews.Add(interview);

        AddActivity(JobApplicationActivity.CreateInterviewScheduled(
            Id,
            interviewType,
            scheduledAt));

        return interview;
    }

    public JobInterview? RecordInterviewResult(
        Guid interviewId,
        InterviewState state,
        DateTime? rescheduledAt,
        string? note)
    {
        JobInterview? interview = _interviews.SingleOrDefault(item => item.Id == interviewId);

        if (interview is null)
        {
            return null;
        }

        interview.RecordResult(state);

        JobInterview? replacementInterview = null;

        if (state == InterviewState.Postponed)
        {
            if (rescheduledAt is null)
            {
                throw new ArgumentException("A new date is required when postponing an interview.", nameof(rescheduledAt));
            }

            replacementInterview = interview.CreateRescheduledInterview(rescheduledAt.Value);
            _interviews.Add(replacementInterview);
        }

        AddActivity(JobApplicationActivity.CreateInterviewResult(Id, state, note));

        return replacementInterview ?? interview;
    }

    private void AddActivity(JobApplicationActivity activity)
    {
        _activities.Add(activity);
        if (activity.OccurredAt > LastActivityAt)
        {
            LastActivityAt = activity.OccurredAt;
        }
    }

    private static bool RequiresAppliedOn(ApplicationStatus status)
    {
        return status is not ApplicationStatus.Draft and not ApplicationStatus.Planned;
    }

    private IReadOnlyList<ApplicationStatus> GetAllowedNextStatuses()
    {
        return Status switch
        {
            ApplicationStatus.Draft => [ApplicationStatus.Planned, ApplicationStatus.Applied, ApplicationStatus.Archived],
            ApplicationStatus.Planned => [ApplicationStatus.Applied, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
            ApplicationStatus.Applied => [ApplicationStatus.InProcess, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
            ApplicationStatus.InProcess => [ApplicationStatus.OfferReceived, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
            ApplicationStatus.OfferReceived => [ApplicationStatus.Accepted, ApplicationStatus.Declined, ApplicationStatus.Withdrawn, ApplicationStatus.Archived],
            ApplicationStatus.Accepted or ApplicationStatus.Declined or ApplicationStatus.Rejected or ApplicationStatus.Withdrawn => [ApplicationStatus.Archived],
            _ => []
        };
    }
}
