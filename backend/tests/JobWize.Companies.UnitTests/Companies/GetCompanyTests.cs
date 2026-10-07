using FluentAssertions;
using JobWize.Modules.Companies.Application;
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

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
