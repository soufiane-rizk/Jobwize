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

    [Fact]
    public void Approve_Should_Promote_Company_And_Record_Review_Metadata()
    {
        Guid candidateId = Guid.NewGuid();
        Guid reviewerId = Guid.NewGuid();
        DateTime reviewedAt = DateTime.UtcNow;
        Company company = Company.CreatePrivate(candidateId, "acme", null, null, null, []);

        company.UpdateBasicInformation("Acme", "https://acme.example", "Technology", "Curated description.");
        company.Approve(reviewerId, reviewedAt, "Corrected branding.");

        company.Visibility.Should().Be(CompanyVisibility.Shared);
        company.Name.Should().Be("Acme");
        company.ReviewedByUserId.Should().Be(reviewerId);
        company.ReviewedAt.Should().Be(reviewedAt);
        company.ReviewReason.Should().Be("Corrected branding.");
    }

    [Fact]
    public void Reject_Should_Keep_Company_Private_And_Require_A_Reason()
    {
        Company company = Company.CreatePrivate(Guid.NewGuid(), "Acme", null, null, null, []);

        Action action = () => company.Reject(Guid.NewGuid(), DateTime.UtcNow, "");

        action.Should().Throw<ArgumentException>();

        company.Reject(Guid.NewGuid(), DateTime.UtcNow, "Insufficient information.");
        company.Visibility.Should().Be(CompanyVisibility.Private);
        company.ReviewReason.Should().Be("Insufficient information.");
    }
}
