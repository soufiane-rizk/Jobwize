using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class ChangeJobApplicationStatus
{
    public const string Route = "/api/applications/{Id}/status";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] ApplicationStatus Status,
        [property: HttpBody] DateOnly? AppliedOn,
        [property: HttpBody] string? Note);
}
