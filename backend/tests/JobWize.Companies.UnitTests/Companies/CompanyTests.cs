using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Domain;

namespace JobWize.Companies.UnitTests.Companies;

public sealed class CompanyTests
{
    [Fact]
    public void CreatePrivate_Should_Assign_Owner_And_Private_Visibility()
    {
        Guid candidateId = Guid.NewGuid();

        Company company = Company.CreatePrivate(
            candidateId,
            "  Acme  ",
            "https://acme.example",
            "Technology",
            null,
            [("Casablanca HQ", "Casablanca", "Morocco", null)]);

        company.Name.Should().Be("Acme");
        company.Visibility.Should().Be(CompanyVisibility.Private);
        company.CreatedByCandidateId.Should().Be(candidateId);
        company.Locations.Should().ContainSingle();
        company.Locations.Single().CompanyId.Should().Be(company.Id);
    }

    [Fact]
    public void AddLocation_Should_Require_Its_Identity_Fields()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.AddLocation("", "Casablanca", "Morocco", null);

        action.Should().Throw<ArgumentException>();
    }
}
