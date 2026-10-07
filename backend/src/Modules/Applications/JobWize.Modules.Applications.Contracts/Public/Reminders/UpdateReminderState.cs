using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Reminders;

public static class UpdateReminderState
{
    public const string Route = "/api/applications/{ApplicationId}/reminders/{ReminderId}/state";

    public sealed record Request(
        [property: HttpRoute] Guid ApplicationId,
        [property: HttpRoute] Guid ReminderId,
        [property: HttpBody] ReminderState State);
}
