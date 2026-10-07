using FluentAssertions;
using JobWize.Modules.Identity.Contracts.Events.Authentication;
using JobWize.Modules.Identity.Application.Authentication;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Domain.Enums;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Infrastructure.Time;
using IdentityApplication = JobWize.Modules.Identity.Application;
using IdentityContracts = JobWize.Modules.Identity.Contracts.Public.Authentication;

namespace JobWize.Identity.UnitTests.Authentication;

public sealed class ChangePasswordTests
{
    [Fact]
    public async Task HandleAsync_Should_Change_Password_Revoke_Existing_Sessions_And_Return_A_Fresh_Session()
    {
        DateTime now = new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);
        User user = User.CreateSuperAdmin("admin@example.com", "hashed-temporary-password");
        user.CreateRefreshToken("first-token", now.AddDays(1));
        user.CreateRefreshToken("second-token", now.AddDays(1));
        var passwordHasher = new FakePasswordHasher();
        var authenticationSessionService = new FakeAuthenticationSessionService();
        var dispatcher = new FakeDispatcher();
        var handler = new IdentityApplication.Authentication.ChangePassword.Handler(
            new FakeUserRepository(user),
            new FakeUserContext(user.Id),
            passwordHasher,
            authenticationSessionService,
            new FakeClock(now),
            dispatcher);

        Result<IdentityContracts.AuthenticationResponse> result = await handler.HandleAsync(
            new IdentityApplication.Authentication.ChangePassword.Command("temporary-password", "new-password", "new-password"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        passwordHasher.HashedPassword.Should().Be("new-password");
        user.PasswordHash.Should().Be("hashed-new-password");
        user.MustChangePassword.Should().BeFalse();
        user.RefreshTokens.Should().OnlyContain(token => token.IsRevoked);
        authenticationSessionService.AuthenticatedUser.Should().BeSameAs(user);
        result.Value.AccessToken.Should().Be("fresh-access-token");
        result.Value.RefreshToken.Should().Be("fresh-refresh-token");
        dispatcher.PublishedNotification.Should().BeOfType<PasswordChanged>()
            .Which.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_An_Incorrect_Current_Password_Without_Changing_Anything()
    {
        DateTime now = new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);
        User user = User.CreateSuperAdmin("admin@example.com", "hashed-temporary-password");
        user.CreateRefreshToken("active-token", now.AddDays(1));
        var passwordHasher = new FakePasswordHasher();
        var authenticationSessionService = new FakeAuthenticationSessionService();
        var dispatcher = new FakeDispatcher();
        var handler = new IdentityApplication.Authentication.ChangePassword.Handler(
            new FakeUserRepository(user),
            new FakeUserContext(user.Id),
            passwordHasher,
            authenticationSessionService,
            new FakeClock(now),
            dispatcher);

        Result<IdentityContracts.AuthenticationResponse> result = await handler.HandleAsync(
            new IdentityApplication.Authentication.ChangePassword.Command("wrong-password", "new-password", "new-password"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.InvalidCurrentPassword");
        user.PasswordHash.Should().Be("hashed-temporary-password");
        user.MustChangePassword.Should().BeTrue();
        user.RefreshTokens.Should().OnlyContain(token => token.IsActive(now));
        passwordHasher.HashedPassword.Should().BeNull();
        authenticationSessionService.AuthenticatedUser.Should().BeNull();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(user.Id == userId ? user : null);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task SaveAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId => userId;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? HashedPassword { get; private set; }
        public string Hash(string password) => "hashed-" + (HashedPassword = password);
        public bool Verify(string password, string passwordHash) => password == "temporary-password" && passwordHash == "hashed-temporary-password";
    }

    private sealed class FakeAuthenticationSessionService : IAuthenticationSessionService
    {
        public User? AuthenticatedUser { get; private set; }

        public Task<AuthenticationSession> AuthenticateAsync(User user, CancellationToken cancellationToken)
        {
            AuthenticatedUser = user;
            return Task.FromResult(new AuthenticationSession(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Role,
                "fresh-access-token",
                "fresh-refresh-token",
                DateTime.UtcNow.AddMinutes(15),
                DateTime.UtcNow.AddDays(30)));
        }

        public Task<Result<AuthenticationSession>> RefreshAsync(User user, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeClock(DateTime now) : IClock
    {
        public DateTime UtcNow => now;
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public INotification? PublishedNotification { get; private set; }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;
            return Task.CompletedTask;
        }
    }
}
