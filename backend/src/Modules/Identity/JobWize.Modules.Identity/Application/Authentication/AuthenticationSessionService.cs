using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Domain.Entities;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Infrastructure.Time;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Application.Authentication
{
    public sealed record AuthenticationSession(
        Guid UserId,    
        string FirstName,
        string LastName,
        Domain.Enums.UserRole Role,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt);

    public interface IAuthenticationSessionService
    {
        Task<AuthenticationSession> AuthenticateAsync(Domain.User user, CancellationToken cancellationToken);
    }

    internal sealed class AuthenticationSessionService : IAuthenticationSessionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IClock _clock;
        private readonly JwtOptions _jwtOptions;

        public AuthenticationSessionService(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IRefreshTokenGenerator refreshTokenGenerator,
            IClock clock,
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _refreshTokenGenerator = refreshTokenGenerator;
            _clock = clock;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<AuthenticationSession> AuthenticateAsync(Domain.User user, CancellationToken cancellationToken)
        {
            DateTime refreshTokenExpiresAt = _clock.UtcNow.Add(_jwtOptions.RefreshTokenLifetime);

            RefreshToken refreshToken = user.CreateRefreshToken(
                _refreshTokenGenerator.Generate(),
                refreshTokenExpiresAt);

            await _userRepository.SaveAsync(user, cancellationToken);

            AccessToken accessToken = _jwtProvider.Generate(user);

            return new AuthenticationSession(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Role,
                accessToken.Value,
                refreshToken.Token,
                accessToken.ExpiresAt,
                refreshToken.ExpiresAt);
        }
    }
}
