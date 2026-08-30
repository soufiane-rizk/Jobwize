using JobWize.Shared.Contracts.Http.Attributes;
using JobWize.Modules.Applications.Contracts.Public.Interviews;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class GetJobApplication
{
    public const string Route = "/api/applications/{Id}";

    public sealed record Request([property: HttpRoute] Guid Id);

    public sealed record ActivityItem(
        Guid Id,
        ApplicationActivityType Type,
        ApplicationStatus? Status,
        DateTime OccurredAt,
        string? Note);

    public sealed record InterviewParticipantItem(
        Guid Id,
        string Name,
        string? RoleTitle);

    public sealed record InterviewItem(
        Guid Id,
        InterviewType Type,
        InterviewState State,
        DateTime ScheduledAt,
        int? DurationMinutes,
        InterviewFormat Format,
        string? Location,
        string? PreparationNotes,
        IReadOnlyList<InterviewParticipantItem> Participants);

    public sealed record CvSubmissionDocumentItem(
        Guid FileId,
        string FileName,
        string ContentType,
        long SizeBytes);

    public sealed record CvSubmissionItem(
        Guid Id,
        DateTime SentAt,
        CvSubmissionMethod Method,
        string? Notes,
        Guid? CompanyContactId,
        Guid? CompanyLocationId,
        string? ContactName,
        string? ContactRoleTitle,
        string? ContactEmail,
        string? ContactPhoneNumber,
        IReadOnlyList<CvSubmissionDocumentItem> Documents);

    public sealed record Response(
        Guid Id,
        Guid? CompanyId,
        string CompanyName,
        string? CompanyLocationLabel,
        string? RoleTitle,
        ApplicationKind Kind,
        ApplicationStatus Status,
        DateOnly? AppliedOn,
        string? SourceUrl,
        string? Notes,
        IReadOnlyList<ActivityItem> Activities,
        IReadOnlyList<InterviewItem> Interviews,
        IReadOnlyList<CvSubmissionItem> CvSubmissions,
        IReadOnlyList<ApplicationStatus> AllowedNextStatuses);
}
