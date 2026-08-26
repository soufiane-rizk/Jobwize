using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Users;

public static class CreateAdmin
{
    public const string Route = "/api/identity/users/admins";
    public sealed record Request(
        [property: HttpBody] string Email,
        [property: HttpBody] string TemporaryPassword,
        [property: HttpBody] string FirstName,
        [property: HttpBody] string LastName);
    public sealed record Response(Guid UserId);
}
