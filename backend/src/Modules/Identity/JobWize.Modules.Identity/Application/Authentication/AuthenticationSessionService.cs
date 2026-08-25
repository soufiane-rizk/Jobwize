using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Domain.Entities;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Infrastructure.Time;
using Microsoft.Extensions.Options;

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

        Task<Result<AuthenticationSession>> RefreshAsync(Domain.User user, string refreshToken, CancellationToken cancellationToken);
    }

    internal sealed class AuthenticationSessionService : IAuthenticationSessionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IClock _clock;
        private readonly JwtOptions _jwtOptions;

        public AuthenticationSessionService(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenHasher refreshTokenHasher,
            IClock clock,
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenHasher = refreshTokenHasher;
            _clock = clock;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<AuthenticationSession> AuthenticateAsync(Domain.User user, CancellationToken cancellationToken)
        {
            DateTime refreshTokenExpiresAt = _clock.UtcNow.Add(_jwtOptions.RefreshTokenLifetime);
            string refreshTokenValue = _refreshTokenGenerator.Generate();

            RefreshToken refreshToken = user.CreateRefreshToken(
                _refreshTokenHasher.Hash(refreshTokenValue),
                refreshTokenExpiresAt);

            await _userRepository.SaveAsync(user, cancellationToken);

            AccessToken accessToken = _jwtProvider.Generate(user);

            return new AuthenticationSession(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Role,
                accessToken.Value,
                refreshTokenValue,
                accessToken.ExpiresAt,
                refreshToken.ExpiresAt);
        }

        public async Task<Result<AuthenticationSession>> RefreshAsync(Domain.User user, string refreshToken, CancellationToken cancellationToken)
        {
            DateTime now = _clock.UtcNow;
            RefreshToken? existingToken = user.FindRefreshToken(_refreshTokenHasher.Hash(refreshToken));

            if (existingToken is null || existingToken.IsRevoked)
                return Result<AuthenticationSession>.Failure(IdentityErrors.RefreshTokenInvalid);

            if (existingToken.IsExpired(now))
                return Result<AuthenticationSession>.Failure(IdentityErrors.RefreshTokenExpired);

            user.RevokeRefreshToken(existingToken.TokenHash, now);

            string replacementTokenValue = _refreshTokenGenerator.Generate();
            RefreshToken replacementToken = user.CreateRefreshToken(
                _refreshTokenHasher.Hash(replacementTokenValue),
                now.Add(_jwtOptions.RefreshTokenLifetime));

            await _userRepository.SaveAsync(user, cancellationToken);

            AccessToken accessToken = _jwtProvider.Generate(user);

            return Result<AuthenticationSession>.Success(new AuthenticationSession(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Role,
                accessToken.Value,
                replacementTokenValue,
                accessToken.ExpiresAt,
                replacementToken.ExpiresAt));
        }
    }
}
