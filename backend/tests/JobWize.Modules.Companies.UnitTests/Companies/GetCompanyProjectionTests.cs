using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Internal.Companies;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Domain;
using JobWize.Modules.Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using GetCompanyProjectionFeature = JobWize.Modules.Companies.Application.Companies.GetCompanyProjectionHandler;

namespace JobWize.Modules.Companies.UnitTests.Companies;

public sealed class GetCompanyProjectionTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Tracked_Approval_Before_It_Is_Persisted()
    {
        Company company = Company.CreatePrivate(
            Guid.NewGuid(),
            "Acme",
            null,
            null,
            null,
            [("Casablanca HQ", "Casablanca", "Morocco", null)]);

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();

        Company trackedCompany = await dbContext.Companies
            .SingleAsync(item => item.Id == company.Id);

        trackedCompany.Approve(Guid.NewGuid(), DateTime.UtcNow, null);

        var handler = new GetCompanyProjectionFeature(dbContext);

        GetCompanyProjection.Response projection = await handler.HandleAsync(
            new GetCompanyProjection.Query(company.Id),
            CancellationToken.None);

        projection.Visibility.Should().Be(CompanyVisibility.Shared);
        projection.Locations.Should().ContainSingle();
        projection.Locations[0].Label.Should().Be("Casablanca HQ");
    }
}
