using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using JobWize.Shared.Contracts.Application;
using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public static class RegisterCandidate
    {
        public const string Route = "/api/identity/authentication/register";
        public sealed record Request(
            [property: HttpBody] string Email,
            [property: HttpBody] string Password,
            [property: HttpBody] string FirstName,
            [property: HttpBody] string LastName);
    }
}
