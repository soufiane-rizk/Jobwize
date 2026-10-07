using FluentAssertions;
using JobWize.Modules.Applications.Application.CompanyContacts;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using GetSelectableCompanyContactsContract = JobWize.Modules.Applications.Contracts.Public.CompanyContacts.GetSelectableCompanyContacts;
using GetSelectableCompanyContactsFeature = JobWize.Modules.Applications.Application.CompanyContacts.GetSelectableCompanyContacts;

namespace JobWize.Modules.Applications.UnitTests.CompanyContacts;

public sealed class GetSelectableCompanyContactsTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Shared_And_Candidate_Private_Active_Contacts_Only()
    {
        Guid candidateId = Guid.NewGuid();
        Guid otherCandidateId = Guid.NewGuid();
        Guid companyId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        dbContext.CompanyContactProjections.AddRange(
            CreateContact(companyId, CompanyContactVisibility.Shared, null, true, "Shared contact"),
            CreateContact(companyId, CompanyContactVisibility.Private, candidateId, true, "My contact"),
            CreateContact(companyId, CompanyContactVisibility.Private, otherCandidateId, true, "Other contact"),
            CreateContact(companyId, CompanyContactVisibility.Shared, null, false, "Disabled contact"),
            CreateContact(companyId, CompanyContactVisibility.Private, candidateId, true, "Rejected contact", isRejected: true));
        await dbContext.SaveChangesAsync();

        var handler = new GetSelectableCompanyContactsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        Result<GetSelectableCompanyContactsContract.Response> result = await handler.HandleAsync(
            new GetSelectableCompanyContactsFeature.Query(companyId, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Contacts.Select(contact => contact.Name).Should().BeEquivalentTo(
            ["Shared contact", "My contact"]);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_By_Optional_Location()
    {
        Guid candidateId = Guid.NewGuid();
        Guid companyId = Guid.NewGuid();
        Guid selectedLocationId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        dbContext.CompanyContactProjections.AddRange(
            CreateContact(companyId, CompanyContactVisibility.Shared, null, true, "Matching", selectedLocationId),
            CreateContact(companyId, CompanyContactVisibility.Shared, null, true, "Company-wide"),
            CreateContact(companyId, CompanyContactVisibility.Shared, null, true, "Other", Guid.NewGuid()));
        await dbContext.SaveChangesAsync();

        var handler = new GetSelectableCompanyContactsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        Result<GetSelectableCompanyContactsContract.Response> result = await handler.HandleAsync(
            new GetSelectableCompanyContactsFeature.Query(companyId, selectedLocationId, null),
            CancellationToken.None);

        result.Value.Contacts.Select(contact => contact.Name).Should().BeEquivalentTo(
            ["Matching", "Company-wide"]);
    }

    private static CompanyContactProjection CreateContact(
        Guid companyId,
        CompanyContactVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive,
        string name,
        Guid? companyLocationId = null,
        bool isRejected = false)
    {
        return CompanyContactProjection.Create(
            Guid.NewGuid(),
            companyId,
            companyLocationId,
            name,
            null,
            null,
            null,
            visibility,
            createdByCandidateId,
            isActive,
            isRejected);
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
