using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Reminders;

public static class CreateReminder
{
    public const string Route = "/api/applications/{Id}/reminders";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] ReminderKind Kind,
        [property: HttpBody] Guid? CvSubmissionId,
        [property: HttpBody] Guid? InterviewId,
        [property: HttpBody] string Title,
        [property: HttpBody] DateTime DueAt,
        [property: HttpBody] string? Note);

    public sealed record Response(Guid ReminderId);
}
