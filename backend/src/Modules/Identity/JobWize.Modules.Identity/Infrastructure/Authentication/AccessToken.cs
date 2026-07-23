using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Infrastructure.Authentication
{
    public sealed record AccessToken(string Value, DateTime ExpiresAt);

}
