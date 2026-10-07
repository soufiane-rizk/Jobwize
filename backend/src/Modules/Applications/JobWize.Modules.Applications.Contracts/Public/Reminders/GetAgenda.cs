using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Reminders;

public static class GetAgenda
{
    public const string Route = "/api/applications/agenda";

    public sealed record Request(
        [property: HttpQuery] DateTime From,
        [property: HttpQuery] DateTime To);

    public sealed record Item(
        Guid Id,
        Guid ApplicationId,
        string CompanyName,
        string? RoleTitle,
        DateTime OccursAt,
        string Title,
        string? Note,
        ReminderKind? ReminderKind,
        ReminderState? ReminderState,
        InterviewState? InterviewState);

    public sealed record Response(IReadOnlyList<Item> Items);
}
