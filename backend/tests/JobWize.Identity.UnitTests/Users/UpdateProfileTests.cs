using FluentAssertions;
using JobWize.Modules.Identity.Application.Users;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;

namespace JobWize.Identity.UnitTests.Users;

public sealed class UpdateProfileTests
{
    [Fact]
    public async Task HandleAsync_Should_Update_Only_The_Current_Users_Name()
    {
        User user = User.CreateCandidate("jane@example.com", "hash", "Jane", "Doe");
        var handler = new UpdateProfile.Handler(new FakeUserRepository(user), new FakeUserContext(user.Id));

        Result<bool> result = await handler.HandleAsync(new UpdateProfile.Command("Janet", "Smith"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("Janet");
        user.LastName.Should().Be("Smith");
        user.Email.Should().Be("jane@example.com");
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(userId == user.Id ? user : null);
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<bool> HasSuperAdminAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task SaveAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId => userId;
    }
}
