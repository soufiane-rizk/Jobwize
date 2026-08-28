using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Events.Companies;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Domain;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using CreatePrivateCompanyFeature = JobWize.Modules.Companies.Application.Companies.CreatePrivateCompany;

namespace JobWize.Companies.UnitTests.Companies;

public sealed class CreatePrivateCompanyTests
{
    [Fact]
    public async Task HandleAsync_Should_Save_Private_Company_And_Publish_Event()
    {
        Guid candidateId = Guid.NewGuid();
        var repository = new FakeCompanyRepository();
        var dispatcher = new FakeDispatcher();
        var handler = new CreatePrivateCompanyFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<CreatePrivateCompany.Response> result = await handler.HandleAsync(
            new CreatePrivateCompanyFeature.Command(
                "Acme",
                "https://acme.example",
                "Technology",
                null,
                [new CreatePrivateCompanyFeature.Location("Casablanca HQ", "Casablanca", "Morocco", null)]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.SavedCompany.Should().NotBeNull();
        repository.SavedCompany!.Visibility.Should().Be(CompanyVisibility.Private);
        repository.SavedCompany.CreatedByCandidateId.Should().Be(candidateId);
        dispatcher.PublishedNotification.Should().BeOfType<CompanyCreated>()
            .Which.CompanyId.Should().Be(result.Value.Id);
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        public Company? SavedCompany { get; private set; }

        public Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Company?>(null);
        }

        public Task SaveAsync(Company company, CancellationToken cancellationToken = default)
        {
            SavedCompany = company;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public INotification? PublishedNotification { get; private set; }

        public Task<TResponse> SendAsync<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<TResponse> SendModuleQueryAsync<TResponse>(
            IModuleQuery<TResponse> query,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;
            return Task.CompletedTask;
        }
    }
}
