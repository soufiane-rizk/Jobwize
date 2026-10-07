using FluentAssertions;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Domain.Enums;
using JobWize.Modules.Identity.Infrastructure;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Transactions;

namespace JobWize.Modules.Identity.UnitTests.Infrastructure;

public sealed class InitialSuperAdminBootstrapperTests
{
    [Fact]
    public async Task BootstrapAsync_Should_Create_Hashed_SuperAdmin_When_It_Does_Not_Exist()
    {
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var transactionContext = new FakeTransactionContext();
        var bootstrapper = new InitialSuperAdminBootstrapper(userRepository, passwordHasher, transactionContext);

        bool created = await bootstrapper.BootstrapAsync(
            new InitialSuperAdminOptions { Email = "admin@example.com", TemporaryPassword = "temporary-password" },
            CancellationToken.None);

        created.Should().BeTrue();
        passwordHasher.Password.Should().Be("temporary-password");
        userRepository.SavedUser.Should().NotBeNull();
        userRepository.SavedUser!.Email.Should().Be("admin@example.com");
        userRepository.SavedUser.PasswordHash.Should().Be("hashed-temporary-password");
        userRepository.SavedUser.Role.Should().Be(UserRole.SuperAdmin);
        userRepository.SavedUser.Status.Should().Be(UserStatus.Active);
        userRepository.SavedUser.MustChangePassword.Should().BeTrue();
        transactionContext.Persisted.Should().BeTrue();
    }

    [Fact]
    public async Task BootstrapAsync_Should_Do_Nothing_When_Email_Already_Exists()
    {
        User existingUser = User.CreateCandidate("admin@example.com", "existing-hash", "Jane", "Doe");
        var userRepository = new FakeUserRepository(existingUser);
        var passwordHasher = new FakePasswordHasher();
        var transactionContext = new FakeTransactionContext();
        var bootstrapper = new InitialSuperAdminBootstrapper(userRepository, passwordHasher, transactionContext);

        bool created = await bootstrapper.BootstrapAsync(
            new InitialSuperAdminOptions { Email = "admin@example.com", TemporaryPassword = "temporary-password" },
            CancellationToken.None);

        created.Should().BeFalse();
        passwordHasher.Password.Should().BeNull();
        userRepository.SavedUser.Should().BeNull();
        transactionContext.Persisted.Should().BeFalse();
    }

    [Fact]
    public async Task BootstrapAsync_Should_Do_Nothing_When_Another_SuperAdmin_Already_Exists()
    {
        User existingSuperAdmin = User.CreateSuperAdmin("existing-admin@example.com", "existing-hash");
        var userRepository = new FakeUserRepository(existingSuperAdmin);
        var passwordHasher = new FakePasswordHasher();
        var transactionContext = new FakeTransactionContext();
        var bootstrapper = new InitialSuperAdminBootstrapper(userRepository, passwordHasher, transactionContext);

        bool created = await bootstrapper.BootstrapAsync(
            new InitialSuperAdminOptions { Email = "new-admin@example.com", TemporaryPassword = "temporary-password" },
            CancellationToken.None);

        created.Should().BeFalse();
        passwordHasher.Password.Should().BeNull();
        userRepository.SavedUser.Should().BeNull();
        transactionContext.Persisted.Should().BeFalse();
    }

    private sealed class FakeUserRepository(User? existingUser = null) : IUserRepository
    {
        public User? SavedUser { get; private set; }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.Equals(existingUser?.Email, email, StringComparison.OrdinalIgnoreCase) ? existingUser : null);
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(existingUser?.Role == UserRole.SuperAdmin);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);

        public Task SaveAsync(User user, CancellationToken cancellationToken = default)
        {
            SavedUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? Password { get; private set; }
        public string Hash(string password) => "hashed-" + (Password = password);
        public bool Verify(string password, string passwordHash) => false;
    }

    private sealed class FakeTransactionContext : ITransactionContext
    {
        public bool Persisted { get; private set; }
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task PersistChangesAsync(CancellationToken cancellationToken)
        {
            Persisted = true;
            return Task.CompletedTask;
        }
    }
}
