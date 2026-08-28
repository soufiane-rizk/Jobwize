using JobWize.Shared.Contracts.Http.Attributes;
using JobWize.Modules.Applications.Contracts.Public.Interviews;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class GetJobApplications
{
    public const string Route = "/api/applications";

    public sealed record Request([property: HttpQuery] Guid? CompanyId);

    public sealed record Item(
        Guid Id,
        Guid? CompanyId,
        string CompanyName,
        string? CompanyLocationLabel,
        string? RoleTitle,
        ApplicationKind Kind,
        ApplicationStatus Status,
        DateTime LastActivityAt,
        Guid? LastInterviewId,
        InterviewState? LastInterviewState,
        DateTime? LastInterviewScheduledAt,
        IReadOnlyList<ApplicationStatus> AllowedNextStatuses);

    public sealed record Response(IReadOnlyList<Item> Applications);
}
