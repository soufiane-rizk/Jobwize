using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class CreateJobApplication
{
    public const string Route = "/api/applications";

    public sealed record Request(
        [property: HttpBody] Guid CompanyId,
        [property: HttpBody] Guid? CompanyLocationId,
        [property: HttpBody] string? RoleTitle,
        [property: HttpBody] ApplicationKind Kind,
        [property: HttpBody] ApplicationStatus Status,
        [property: HttpBody] DateOnly? AppliedOn,
        [property: HttpBody] string? SourceUrl,
        [property: HttpBody] string? Notes);

    public sealed record Response(Guid Id);
}
