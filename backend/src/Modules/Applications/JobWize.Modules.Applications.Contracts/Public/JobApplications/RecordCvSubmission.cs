using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class RecordCvSubmission
{
    public const string Route = "/api/applications/{Id}/cv-submissions";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] DateTime SentAt,
        [property: HttpBody] CvSubmissionMethod Method,
        [property: HttpBody] IReadOnlyList<Guid> FileIds,
        [property: HttpBody] Guid? CompanyContactId,
        [property: HttpBody] string? Notes);

    public sealed record Response(Guid SubmissionId);
}
