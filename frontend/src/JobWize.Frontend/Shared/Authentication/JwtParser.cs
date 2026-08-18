using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JobWize.Frontend.Shared.Authentication
{
    internal static class JwtParser
    {
        public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity());
        public static ClaimsPrincipal Parse(string jwt)
        {
            JwtSecurityTokenHandler handler = new();

            JwtSecurityToken token = handler.ReadJwtToken(jwt);

            ClaimsIdentity identity = new(token.Claims, authenticationType: "jwt");

            return new ClaimsPrincipal(identity);
        }
    }
}
