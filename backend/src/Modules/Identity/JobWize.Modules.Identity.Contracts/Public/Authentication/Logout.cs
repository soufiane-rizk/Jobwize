using JobWize.Shared.Contracts.Http.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public static class Logout
    {
        public const string Route = "/api/identity/authentication/logout";
        public sealed record Request([property: HttpBody] string RefreshToken);

    }
}
