using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Events.Companies;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Modules.Companies.Domain;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Security;
using ReviewCompanyFeature = JobWize.Modules.Companies.Application.Companies.ReviewCompany;

namespace JobWize.Companies.UnitTests.Companies;

public sealed class ReviewCompanyTests
{
    [Fact]
    public async Task HandleAsync_Should_Review_Existing_Children_And_Add_Shared_Children()
    {
        Guid candidateId = Guid.NewGuid();
        Guid reviewerId = Guid.NewGuid();
        Company company = Company.CreatePrivate(
            candidateId,
            "acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)],
            [(0, "Invalid contact", null, null, null)]);
        CompanyLocation submittedLocation = company.Locations.Single();
        CompanyContact submittedContact = company.Contacts.Single();
        var repository = new FakeCompanyRepository(company);
        var dispatcher = new FakeDispatcher();
        var handler = new ReviewCompanyFeature.Handler(
            repository,
            new FakeUserContext(reviewerId),
            dispatcher);

        var command = new ReviewCompanyFeature.Command(
            company.Id,
            true,
            null,
            "Acme",
            "https://acme.example",
            "Technology",
            "Updated during review.",
            [
                new JobWize.Modules.Companies.Contracts.Public.Companies.ReviewCompany.Location(
                    submittedLocation.Id,
                    true,
                    null,
                    "Casablanca HQ",
                    "Casablanca",
                    "Morocco",
                    null),
                new JobWize.Modules.Companies.Contracts.Public.Companies.ReviewCompany.Location(
                    null,
                    true,
                    null,
                    null,
                    "Rabat",
                    "Morocco",
                    null)
            ],
            [
                new JobWize.Modules.Companies.Contracts.Public.Companies.ReviewCompany.Contact(
                    submittedContact.Id,
                    false,
                    "The contact is invalid.",
                    0,
                    submittedContact.Name,
                    null,
                    null,
                    null),
                new JobWize.Modules.Companies.Contracts.Public.Companies.ReviewCompany.Contact(
                    null,
                    true,
                    null,
                    1,
                    "Shared recruiter",
                    "Recruiter",
                    "recruiter@acme.example",
                    null)
            ]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.SavedCompany.Should().BeSameAs(company);
        company.Visibility.Should().Be(CompanyVisibility.Shared);
        company.Name.Should().Be("Acme");
        company.Locations.Should().HaveCount(2);
        company.Locations.Should().OnlyContain(location =>
            location.Visibility == CompanyLocationVisibility.Shared);
        company.Contacts.Should().HaveCount(2);
        company.Contacts.Single(contact => contact.Id == submittedContact.Id)
            .Visibility.Should().Be(CompanyContactVisibility.Private);
        company.Contacts.Single(contact => contact.Id != submittedContact.Id)
            .Visibility.Should().Be(CompanyContactVisibility.Shared);
        dispatcher.PublishedNotification.Should().BeOfType<CompanyPromotedToShared>();
    }

    private sealed class FakeCompanyRepository(Company company) : ICompanyRepository
    {
        public Company? SavedCompany { get; private set; }

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Company?>(companyId == company.Id ? company : null);
        }

        public Task SaveAsync(
            Company savedCompany,
            CancellationToken cancellationToken = default)
        {
            SavedCompany = savedCompany;
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
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<TResponse> SendModuleQueryAsync<TResponse>(
            IModuleQuery<TResponse> query,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task PublishAsync(
            INotification notification,
            CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;
            return Task.CompletedTask;
        }
    }
}
