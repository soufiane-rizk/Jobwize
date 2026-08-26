using JobWize.Modules.Identity.Contracts.Public.Authentication;

namespace JobWize.Modules.Identity.Contracts.Public.Users
{
    public static class GetCurrentUser
    {
        public const string Route = "/api/identity/users/me";

        public sealed record Request();

        public sealed record Response(
            Guid UserId,
            string FirstName,
            string LastName,
            string Email,
            UserRole Role,
            string? AvatarUrl,
            bool MustChangePassword);
    }
}
