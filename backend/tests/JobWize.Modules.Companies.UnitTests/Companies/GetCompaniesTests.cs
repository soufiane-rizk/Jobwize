using FluentAssertions;
using JobWize.Modules.Companies.Application.Companies;
using JobWize.Modules.Companies.Domain;
using JobWize.Modules.Companies.Persistence;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using GetCompaniesFeature = JobWize.Modules.Companies.Application.Companies.GetCompanies;

namespace JobWize.Modules.Companies.UnitTests.Companies;

public sealed class GetCompaniesTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Shared_And_Current_Candidates_Companies_Only()
    {
        Guid candidateId = Guid.NewGuid();
        Guid anotherCandidateId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CompaniesDbContext(options);

        dbContext.Companies.AddRange(
            Company.CreateShared("Shared company", null, null, null),
            Company.CreatePrivate(candidateId, "My private company", null, null, null, []),
            Company.CreatePrivate(anotherCandidateId, "Another private company", null, null, null, []));
        await dbContext.SaveChangesAsync();

        var handler = new GetCompaniesFeature.Handler(dbContext, new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(new GetCompaniesFeature.Query(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Companies.Select(company => company.Name).Should().BeEquivalentTo(
            ["Shared company", "My private company"]);
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
