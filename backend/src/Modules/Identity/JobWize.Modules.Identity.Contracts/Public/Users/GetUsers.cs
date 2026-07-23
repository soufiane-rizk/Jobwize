using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Public.Users
{
    public static class GetUsers
    {
        public sealed record Request();

        public sealed record UserDto(
            Guid UserId,
            string FirstName,
            string LastName,
            string Email,
            string Role);
        public sealed record Response(IReadOnlyList<UserDto> Users);
    }
}
