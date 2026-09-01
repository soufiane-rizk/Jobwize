using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Interviews;

public static class ScheduleInterview
{
    public const string Route = "/api/applications/{Id}/interviews";

    public sealed record ManualParticipant(string Name, string? RoleTitle);

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] InterviewType Type,
        [property: HttpBody] DateTime ScheduledAt,
        [property: HttpBody] int? DurationMinutes,
        [property: HttpBody] InterviewFormat Format,
        [property: HttpBody] string? Location,
        [property: HttpBody] string? PreparationNotes,
        [property: HttpBody] IReadOnlyList<Guid> CompanyContactIds,
        [property: HttpBody] IReadOnlyList<ManualParticipant> ManualParticipants);

    public sealed record Response(Guid InterviewId);
}
