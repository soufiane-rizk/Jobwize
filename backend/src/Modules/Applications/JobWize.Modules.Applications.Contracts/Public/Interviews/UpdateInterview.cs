using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Interviews;

public static class UpdateInterview
{
    public const string Route = "/api/applications/{ApplicationId}/interviews/{InterviewId}";

    public sealed record ManualParticipant(string Name, string? RoleTitle);

    public sealed record Request(
        [property: HttpRoute] Guid ApplicationId,
        [property: HttpRoute] Guid InterviewId,
        [property: HttpBody] InterviewType Type,
        [property: HttpBody] DateTime ScheduledAt,
        [property: HttpBody] int? DurationMinutes,
        [property: HttpBody] InterviewFormat Format,
        [property: HttpBody] string? Location,
        [property: HttpBody] string? PreparationNotes,
        [property: HttpBody] IReadOnlyList<Guid> CompanyContactIds,
        [property: HttpBody] IReadOnlyList<ManualParticipant> ManualParticipants);
}
