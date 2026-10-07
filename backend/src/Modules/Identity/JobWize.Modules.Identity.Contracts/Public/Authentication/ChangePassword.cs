using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication;

public static class ChangePassword
{
    public const string Route = "/api/identity/authentication/change-password";

    public sealed record Request(
        [property: HttpBody] string CurrentPassword,
        [property: HttpBody] string NewPassword,
        [property: HttpBody] string ConfirmPassword);
}
