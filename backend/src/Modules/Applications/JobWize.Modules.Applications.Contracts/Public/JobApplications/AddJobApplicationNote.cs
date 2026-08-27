using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class AddJobApplicationNote
{
    public const string Route = "/api/applications/{Id}/notes";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] string Note);
}
