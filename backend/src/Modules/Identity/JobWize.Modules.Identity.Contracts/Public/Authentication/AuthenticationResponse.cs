using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public enum UserRole
    {
        Candidate,
        Admin,
        SuperAdmin
    }

    public sealed record AuthenticationResponse(
         Guid UserId,

         string FirstName,
         string LastName,

         UserRole Role,

         string AccessToken,
         string RefreshToken,

         DateTime AccessTokenExpiresAt,
         DateTime RefreshTokenExpiresAt);
}
