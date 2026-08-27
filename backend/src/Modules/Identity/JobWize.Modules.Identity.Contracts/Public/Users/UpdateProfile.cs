using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Users;

public static class UpdateProfile
{
    public const string Route = "/api/identity/users/me";

    public sealed record Request(
        [property: HttpBody] string FirstName,
        [property: HttpBody] string LastName);
}
