using FluentAssertions;
using JobWize.Modules.Identity.Application.Users;
using JobWize.Modules.Identity.Contracts.Events.Users;
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
using UserContracts = JobWize.Modules.Identity.Contracts.Public.Users;

namespace JobWize.Identity.UnitTests.Users;

public sealed class UserManagementTests
{
    [Fact]
    public async Task CreateAdmin_Should_Create_Admin_With_A_Temporary_Password_And_Publish_Event()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var dispatcher = new FakeDispatcher();
        Guid actorId = Guid.NewGuid();
        var handler = new CreateAdmin.Handler(repository, passwordHasher, new FakeUserContext(actorId), dispatcher);

        Result<UserContracts.CreateAdmin.Response> result = await handler.HandleAsync(
            new CreateAdmin.Command("admin@example.com", "temporary-password", "Ada", "Admin"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.SavedUser.Should().NotBeNull();
        repository.SavedUser!.Role.Should().Be(UserRole.Admin);
        repository.SavedUser.MustChangePassword.Should().BeTrue();
        repository.SavedUser.PasswordHash.Should().Be("hashed-temporary-password");
        dispatcher.PublishedNotification.Should().BeOfType<UserCreated>()
            .Which.CreatedByUserId.Should().Be(actorId);
    }

    [Fact]
    public async Task CreateAdmin_Should_Reject_An_Existing_Email()
    {
        var existing = User.CreateCandidate("admin@example.com", "hash", "Ada", "Candidate");
        var handler = new CreateAdmin.Handler(new FakeUserRepository(existing), new FakePasswordHasher(), new FakeUserContext(Guid.NewGuid()), new FakeDispatcher());

        Result<UserContracts.CreateAdmin.Response> result = await handler.HandleAsync(
            new CreateAdmin.Command("admin@example.com", "temporary-password", "Ada", "Admin"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.EmailAlreadyExists");
    }

    [Fact]
    public async Task Suspend_Should_Require_Confirmation_Before_Changing_The_User()
    {
        DateTime now = new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);
        User candidate = User.CreateCandidate("candidate@example.com", "hash", "Jane", "Doe");
        candidate.CreateRefreshToken("active-token", now.AddDays(1));
        var handler = new SuspendUser.Handler(new FakeUserRepository(candidate), new FakeUserContext(Guid.NewGuid()), new FakeClock(now), new FakeDispatcher());

        Result<bool> result = await handler.HandleAsync(new SuspendUser.Command(candidate.Id, false, []), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.NeedsConfirmation.Should().BeTrue();
        result.Confirmations.Should().ContainSingle().Which.Key.Should().Be(UserContracts.SuspendUser.ConfirmationKey);
        candidate.Status.Should().Be(UserStatus.Active);
        candidate.RefreshTokens.Should().OnlyContain(token => token.IsActive(now));
    }

    [Fact]
    public async Task Suspend_Should_Revoke_Sessions_After_Confirmation_And_Publish_Event()
    {
        DateTime now = new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);
        User candidate = User.CreateCandidate("candidate@example.com", "hash", "Jane", "Doe");
        candidate.CreateRefreshToken("active-token", now.AddDays(1));
        Guid actorId = Guid.NewGuid();
        var dispatcher = new FakeDispatcher();
        var handler = new SuspendUser.Handler(new FakeUserRepository(candidate), new FakeUserContext(actorId), new FakeClock(now), dispatcher);

        Result<bool> result = await handler.HandleAsync(
            new SuspendUser.Command(candidate.Id, false, [UserContracts.SuspendUser.ConfirmationKey]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        candidate.Status.Should().Be(UserStatus.Suspended);
        candidate.RefreshTokens.Should().OnlyContain(token => token.IsRevoked);
        dispatcher.PublishedNotification.Should().BeOfType<UserSuspended>()
            .Which.SuspendedByUserId.Should().Be(actorId);
    }

    [Fact]
    public async Task Admin_Cannot_Manage_Another_Admin_But_SuperAdmin_Can()
    {
        User admin = User.CreateAdmin("admin@example.com", "hash", "Ada", "Admin");
        var repository = new FakeUserRepository(admin);
        var handler = new SuspendUser.Handler(repository, new FakeUserContext(Guid.NewGuid()), new FakeClock(DateTime.UtcNow), new FakeDispatcher());

        Result<bool> adminResult = await handler.HandleAsync(
            new SuspendUser.Command(admin.Id, false, [UserContracts.SuspendUser.ConfirmationKey]),
            CancellationToken.None);
        Result<bool> superAdminResult = await handler.HandleAsync(
            new SuspendUser.Command(admin.Id, true, [UserContracts.SuspendUser.ConfirmationKey]),
            CancellationToken.None);

        adminResult.Error.Code.Should().Be("Identity.UserManagementForbidden");
        superAdminResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivate_Should_Restore_A_Suspended_Managed_User_And_Publish_Event()
    {
        User candidate = User.CreateCandidate("candidate@example.com", "hash", "Jane", "Doe");
        candidate.Suspend(DateTime.UtcNow);
        Guid actorId = Guid.NewGuid();
        var dispatcher = new FakeDispatcher();
        var handler = new ReactivateUser.Handler(new FakeUserRepository(candidate), new FakeUserContext(actorId), dispatcher);

        Result<bool> result = await handler.HandleAsync(new ReactivateUser.Command(candidate.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        candidate.Status.Should().Be(UserStatus.Active);
        dispatcher.PublishedNotification.Should().BeOfType<UserReactivated>()
            .Which.ReactivatedByUserId.Should().Be(actorId);
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        private readonly List<User> _users = [.. users];
        public User? SavedUser { get; private set; }
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(_users.SingleOrDefault(user => user.Id == userId));
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(_users.SingleOrDefault(user => user.Email == email));
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task SaveAsync(User user, CancellationToken cancellationToken = default) { SavedUser = user; _users.Add(user); return Task.CompletedTask; }
    }

    private sealed class FakePasswordHasher : IPasswordHasher { public string Hash(string password) => $"hashed-{password}"; public bool Verify(string password, string passwordHash) => false; }
    private sealed class FakeUserContext(Guid userId) : IUserContext { public Guid UserId => userId; }
    private sealed class FakeClock(DateTime now) : IClock { public DateTime UtcNow => now; }
    private sealed class FakeDispatcher : IDispatcher
    {
        public INotification? PublishedNotification { get; private set; }
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default) { PublishedNotification = notification; return Task.CompletedTask; }
    }
}
