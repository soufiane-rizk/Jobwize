using FluentAssertions;
using JobWize.Modules.Identity.Application.Authentication;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using AuthenticationContracts = JobWize.Modules.Identity.Contracts.Public.Authentication;

namespace JobWize.Modules.Identity.UnitTests.Authentication;

public sealed class LoginTests
{
    [Fact]
    public async Task HandleAsync_Should_Reject_A_Suspended_User_Before_Creating_A_Session()
    {
        User user = User.CreateCandidate("candidate@example.com", "hash", "Jane", "Doe");
        user.Suspend(DateTime.UtcNow);
        var sessionService = new FakeAuthenticationSessionService();
        var handler = new Login.Handler(
            new FakeUserRepository(user),
            new FakePasswordHasher(),
            sessionService,
            new FakeDispatcher());

        Result<AuthenticationContracts.AuthenticationResponse> result = await handler.HandleAsync(
            new Login.Command("candidate@example.com", "password"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.AccountSuspended");
        sessionService.WasCalled.Should().BeFalse();
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(user.Email == email ? user : null);
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task SaveAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher { public string Hash(string password) => password; public bool Verify(string password, string passwordHash) => true; }
    private sealed class FakeAuthenticationSessionService : IAuthenticationSessionService
    {
        public bool WasCalled { get; private set; }
        public Task<AuthenticationSession> AuthenticateAsync(User user, CancellationToken cancellationToken) { WasCalled = true; throw new NotSupportedException(); }
        public Task<Result<AuthenticationSession>> RefreshAsync(User user, string refreshToken, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
    private sealed class FakeDispatcher : IDispatcher
    {
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
