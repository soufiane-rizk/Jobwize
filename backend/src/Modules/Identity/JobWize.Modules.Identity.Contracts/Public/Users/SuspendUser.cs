using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Users;

public static class SuspendUser
{
    public const string Route = "/api/identity/users/suspend";
    public const string ConfirmationKey = "suspend-user";
    public sealed record Request(
        [property: HttpBody] Guid UserId,
        [property: HttpBody] IReadOnlyList<string> ConfirmedActions);
}
