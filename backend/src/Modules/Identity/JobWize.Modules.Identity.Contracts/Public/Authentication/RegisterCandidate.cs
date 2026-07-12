using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using JobWize.Shared.Contracts.Application;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public static class RegisterCandidate
    {
        public sealed record Request(
           string Email,
           string Password,
           string FirstName,
           string LastName);
    }
}
