using FluentAssertions;
using JobWize.Modules.Companies.Application;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Domain;
using JobWize.Modules.Companies.Persistence;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using GetCompanyFeature = JobWize.Modules.Companies.Application.Companies.GetCompany;

namespace JobWize.Companies.UnitTests.Companies;

public sealed class GetCompanyTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_A_Shared_Company()
    {
        var candidateId = Guid.NewGuid();
        Company company = Company.CreateShared("Acme", null, null, null);

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var handler = new GetCompanyFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetCompanyFeature.Query(company.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme");
    }

    [Fact]
    public async Task HandleAsync_Should_Hide_Another_Candidates_Private_Company()
    {
        var candidateId = Guid.NewGuid();
        Company company = Company.CreatePrivate(
            Guid.NewGuid(),
            "Private company",
            null,
            null,
            null,
            []);

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var handler = new GetCompanyFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetCompanyFeature.Query(company.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(CompaniesErrors.CompanyNotFound);
    }

    [Fact]
    public async Task HandleAsync_Should_Show_A_Rejected_Location_Only_To_Its_Creator()
    {
        Guid candidateId = Guid.NewGuid();
        Company company = Company.CreatePrivate(
            candidateId,
            "Acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)]);
        Guid locationId = company.Locations.Single().Id;
        Guid reviewerId = Guid.NewGuid();
        DateTime reviewedAt = DateTime.UtcNow;
        company.Approve(reviewerId, reviewedAt, null, approvePendingChildren: false);
        company.RejectLocation(locationId, reviewerId, reviewedAt, "Not suitable for the catalogue.");

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var ownerHandler = new GetCompanyFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));
        var otherCandidateHandler = new GetCompanyFeature.Handler(
            dbContext,
            new FakeUserContext(Guid.NewGuid()));

        var ownerResult = await ownerHandler.HandleAsync(
            new GetCompanyFeature.Query(company.Id),
            CancellationToken.None);
        var otherCandidateResult = await otherCandidateHandler.HandleAsync(
            new GetCompanyFeature.Query(company.Id),
            CancellationToken.None);

        ownerResult.Value.Locations.Should().ContainSingle();
        otherCandidateResult.Value.Locations.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Hide_A_Disabled_Location_From_Every_Candidate()
    {
        Guid candidateId = Guid.NewGuid();
        Company company = Company.CreateShared("Acme", null, null, null);
        CompanyLocation location = company.AddSharedLocation(
            null,
            "Casablanca",
            "Morocco",
            null);
        company.SetLocationActive(location.Id, false);

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        var handler = new GetCompanyFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));
        var result = await handler.HandleAsync(
            new GetCompanyFeature.Query(company.Id),
            CancellationToken.None);

        result.Value.Locations.Should().BeEmpty();
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
