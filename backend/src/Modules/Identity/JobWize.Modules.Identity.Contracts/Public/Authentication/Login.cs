using JobWize.Shared.Contracts.Http.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public static class Login
    {
        public const string Route = "/api/identity/authentication/login";
        public sealed record Request(
            [property: HttpBody] string Username,
            [property: HttpBody] string Password);
    }
}
