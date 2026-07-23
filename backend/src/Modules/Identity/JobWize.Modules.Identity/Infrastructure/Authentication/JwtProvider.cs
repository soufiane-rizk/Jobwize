using JobWize.Modules.Identity.Domain;
using JobWize.Shared.Infrastructure.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobWize.Modules.Identity.Infrastructure.Authentication
{

    public interface IJwtProvider
    {
        AccessToken Generate(User user);

    }
    internal class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;
        private readonly IClock _clock;
        private static readonly JwtSecurityTokenHandler TokenHandler = new();

        public JwtProvider(IOptions<JwtOptions> options, IClock clock)
        {
            _options = options.Value;
            _clock = clock;
        }

        public AccessToken Generate(User user)
        {
            var now = _clock.UtcNow;
            var expiresAt = now.Add(_options.AccessTokenLifetime);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

            var token = TokenHandler.WriteToken(jwt);

            return new AccessToken(token, expiresAt);
        }
    }
}
