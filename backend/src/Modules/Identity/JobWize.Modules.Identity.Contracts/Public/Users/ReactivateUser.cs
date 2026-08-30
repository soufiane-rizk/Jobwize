using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Users;

public static class ReactivateUser
{
    public const string Route = "/api/identity/users/reactivate";
    public sealed record Request([property: HttpBody] Guid UserId);
}
