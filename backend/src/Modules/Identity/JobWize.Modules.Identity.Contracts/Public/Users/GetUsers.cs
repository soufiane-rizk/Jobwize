using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Public.Users
{
    public static class GetUsers
    {
        public const string Route = "/api/identity/users";

        public sealed record Request();

        public sealed record UserDto(
            Guid UserId,
            string FirstName,
            string LastName,
            string Email,
            JobWize.Modules.Identity.Contracts.Public.Authentication.UserRole Role,
            UserStatus Status,
            bool MustChangePassword,
            DateTime CreatedAt);
        public sealed record Response(IReadOnlyList<UserDto> Users);
    }
}
