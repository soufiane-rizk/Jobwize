using FluentAssertions;
using FluentValidation.Results;
using JobWize.Modules.Identity.Application.Authentication;
using JobWize.Modules.Identity.Contracts.Events.Authentication;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Domain.Enums;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using IdentityApplication = JobWize.Modules.Identity.Application;
using IdentityContracts = JobWize.Modules.Identity.Contracts.Public.Authentication;

namespace JobWize.Identity.UnitTests.Authentication;

public sealed class RegisterCandidateTests
{
    [Theory]
    [InlineData("", "password123", "Jane", "Doe", "Email")]
    [InlineData("not-an-email", "password123", "Jane", "Doe", "Email")]
    [InlineData("jane@example.com", "short", "Jane", "Doe", "Password")]
    [InlineData("jane@example.com", "password123", "", "Doe", "FirstName")]
    [InlineData("jane@example.com", "password123", "Jane", "", "LastName")]
    public void Validator_Should_Reject_Invalid_Registration_Data(
        string email,
        string password,
        string firstName,
        string lastName,
        string expectedProperty)
    {
        var validator = new IdentityApplication.User.RegisterCandidate.Validator();

        ValidationResult validationResult = validator.Validate(
            new IdentityApplication.User.RegisterCandidate.Command(email, password, firstName, lastName));

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(error => error.PropertyName == expectedProperty);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Candidate_Authenticate_And_Publish_Event()
    {
        FakeUserRepository userRepository = new();
        FakePasswordHasher passwordHasher = new();
        FakeAuthenticationSessionService authenticationSessionService = new();
        FakeDispatcher dispatcher = new();
        var handler = new IdentityApplication.User.RegisterCandidate.Handler(
            userRepository,
            passwordHasher,
            authenticationSessionService,
            dispatcher);

        Result<IdentityContracts.AuthenticationResponse> result = await handler.HandleAsync(
            new IdentityApplication.User.RegisterCandidate.Command("jane@example.com", "password123", "Jane", "Doe"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Role.Should().Be(IdentityContracts.UserRole.Candidate);
        passwordHasher.Password.Should().Be("password123");
        authenticationSessionService.AuthenticatedUser.Should().NotBeNull();
        authenticationSessionService.AuthenticatedUser!.Email.Should().Be("jane@example.com");
        authenticationSessionService.AuthenticatedUser.PasswordHash.Should().Be("hashed-password123");
        authenticationSessionService.AuthenticatedUser.Role.Should().Be(UserRole.Candidate);
        dispatcher.PublishedNotification.Should().BeOfType<CandidateRegistered>()
            .Which.UserId.Should().Be(result.Value.UserId);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Conflict_Without_Side_Effects_When_Email_Already_Exists()
    {
        User existingUser = User.CreateCandidate("jane@example.com", "existing-hash", "Jane", "Doe");
        FakeUserRepository userRepository = new(existingUser);
        FakePasswordHasher passwordHasher = new();
        FakeAuthenticationSessionService authenticationSessionService = new();
        FakeDispatcher dispatcher = new();
        var handler = new IdentityApplication.User.RegisterCandidate.Handler(
            userRepository,
            passwordHasher,
            authenticationSessionService,
            dispatcher);

        Result<IdentityContracts.AuthenticationResponse> result = await handler.HandleAsync(
            new IdentityApplication.User.RegisterCandidate.Command("jane@example.com", "password123", "Jane", "Doe"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.EmailAlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        passwordHasher.Password.Should().BeNull();
        authenticationSessionService.AuthenticatedUser.Should().BeNull();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    private sealed class FakeUserRepository(User? existingUser = null) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.Equals(existingUser?.Email, email, StringComparison.OrdinalIgnoreCase) ? existingUser : null);

        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);

        public Task SaveAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? Password { get; private set; }

        public string Hash(string password)
        {
            Password = password;
            return $"hashed-{password}";
        }

        public bool Verify(string password, string passwordHash) => false;
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
                "access-token",
                "refresh-token",
                DateTime.UtcNow.AddMinutes(15),
                DateTime.UtcNow.AddDays(30)));
        }

        public Task<Result<AuthenticationSession>> RefreshAsync(User user, string refreshToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
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
