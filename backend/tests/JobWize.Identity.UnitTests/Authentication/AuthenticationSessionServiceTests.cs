using FluentAssertions;
using JobWize.Modules.Identity.Application.Authentication;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Infrastructure.Time;
using Microsoft.Extensions.Options;

namespace JobWize.Identity.UnitTests.Authentication;

public sealed class AuthenticationSessionServiceTests
{
    [Fact]
    public async Task RefreshAsync_Should_Rotate_Active_Token_And_Return_New_Session()
    {
        DateTime now = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        RefreshTokenHasher hasher = new();
        User user = User.CreateCandidate("user@test.com", "hash", "Jane", "Doe");
        user.CreateRefreshToken(hasher.Hash("old-token"), now.AddDays(1));

        AuthenticationSessionService service = CreateService(now, hasher);

        Result<AuthenticationSession> result = await service.RefreshAsync(user, "old-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().NotBe("old-token");
        user.RefreshTokens.Should().HaveCount(2);
        user.FindRefreshToken("old-token").Should().BeNull();
        user.FindRefreshToken(hasher.Hash("old-token"))!.IsRevoked.Should().BeTrue();
        user.FindRefreshToken(hasher.Hash(result.Value.RefreshToken))!.IsActive(now).Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_Should_Reject_Expired_Token()
    {
        DateTime now = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        RefreshTokenHasher hasher = new();
        User user = User.CreateCandidate("user@test.com", "hash", "Jane", "Doe");
        user.CreateRefreshToken(hasher.Hash("expired-token"), now.AddSeconds(-1));

        Result<AuthenticationSession> result = await CreateService(now, hasher).RefreshAsync(user, "expired-token", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.RefreshTokenExpired");
    }

    [Fact]
    public async Task RefreshAsync_Should_Reject_Revoked_Token()
    {
        DateTime now = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        RefreshTokenHasher hasher = new();
        User user = User.CreateCandidate("user@test.com", "hash", "Jane", "Doe");
        user.CreateRefreshToken(hasher.Hash("revoked-token"), now.AddDays(1));
        user.RevokeRefreshToken(hasher.Hash("revoked-token"), now);

        Result<AuthenticationSession> result = await CreateService(now, hasher).RefreshAsync(user, "revoked-token", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.RefreshTokenInvalid");
    }

    private static AuthenticationSessionService CreateService(DateTime now, IRefreshTokenHasher hasher) => new(
        new FakeUserRepository(), new FakeJwtProvider(now), new FakeRefreshTokenGenerator(), hasher,
        new FakeClock(now), Options.Create(new JwtOptions { Issuer = "issuer", Audience = "audience", SecretKey = new string('a', 32), AccessTokenLifetime = TimeSpan.FromMinutes(15), RefreshTokenLifetime = TimeSpan.FromDays(30) }));

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task SaveAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
    private sealed class FakeJwtProvider(DateTime now) : IJwtProvider { public AccessToken Generate(User user) => new("access-token", now.AddMinutes(15)); }
    private sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator { public string Generate() => "replacement-token"; }
    private sealed class FakeClock(DateTime now) : IClock { public DateTime UtcNow => now; }
}
