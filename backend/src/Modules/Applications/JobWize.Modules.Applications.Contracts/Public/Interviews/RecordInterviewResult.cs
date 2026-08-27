using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Interviews;

public static class RecordInterviewResult
{
    public const string Route = "/api/applications/{ApplicationId}/interviews/{InterviewId}/result";

    public sealed record Request(
        [property: HttpRoute] Guid ApplicationId,
        [property: HttpRoute] Guid InterviewId,
        [property: HttpBody] InterviewState State,
        [property: HttpBody] DateTime? RescheduledAt,
        [property: HttpBody] string? Note);
}
