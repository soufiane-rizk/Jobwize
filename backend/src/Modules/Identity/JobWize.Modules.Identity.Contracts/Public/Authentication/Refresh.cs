using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Identity.Contracts.Public.Authentication
{
    public static class Refresh
    {
        public const string Route = "/api/identity/authentication/refresh";

        public sealed record Request([property: HttpBody] string RefreshToken);
    }
}
