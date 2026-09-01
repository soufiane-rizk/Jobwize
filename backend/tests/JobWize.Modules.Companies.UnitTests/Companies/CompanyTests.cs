using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Companies.UnitTests.Companies;

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
    public void AddLocation_Should_Allow_A_Missing_Label()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        company.AddSharedLocation(null, "Casablanca", "Morocco", null);

        company.Locations.Should().ContainSingle();
        company.Locations.Single().Label.Should().BeNull();
    }

    [Fact]
    public void AddLocation_Should_Require_City_And_Country()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.AddSharedLocation(null, "", "Morocco", null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.LocationCityRequired);
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
    public void Approve_Should_Promote_Contacts_Submitted_With_The_Company()
    {
        Company company = Company.CreatePrivate(
            Guid.NewGuid(),
            "Acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)],
            [(0, "Samira Benali", "Recruiter", "samira@acme.example", null)]);

        company.Approve(Guid.NewGuid(), DateTime.UtcNow, null);

        company.Contacts.Should().ContainSingle();
        company.Contacts.Single().Visibility.Should().Be(CompanyContactVisibility.Shared);
        company.Contacts.Single().ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void Explicit_Child_Review_Should_Keep_Rejected_Data_Private()
    {
        Guid candidateId = Guid.NewGuid();
        Guid reviewerId = Guid.NewGuid();
        DateTime reviewedAt = DateTime.UtcNow;
        Company company = Company.CreatePrivate(
            candidateId,
            "Acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)],
            [(0, "Old contact", null, null, null)]);
        Guid locationId = company.Locations.Single().Id;
        Guid contactId = company.Contacts.Single().Id;

        company.Approve(reviewerId, reviewedAt, null, approvePendingChildren: false);
        company.ApproveLocation(locationId, reviewerId, reviewedAt, null);
        company.RejectContact(contactId, reviewerId, reviewedAt, "The contact is no longer valid.");

        company.Locations.Single().Visibility.Should().Be(CompanyLocationVisibility.Shared);
        company.Contacts.Single().Visibility.Should().Be(CompanyContactVisibility.Private);
        company.Contacts.Single().CreatedByCandidateId.Should().Be(candidateId);
        company.Contacts.Single().ReviewReason.Should().Be("The contact is no longer valid.");
    }

    [Fact]
    public void Disabling_A_Child_Should_Preserve_It_In_The_Aggregate()
    {
        Company company = Company.CreateShared("Acme", null, null, null);
        CompanyLocation location = company.AddSharedLocation(null, "Casablanca", "Morocco", null);
        CompanyContact contact = company.AddSharedContact(
            location.Id,
            "Recruiter",
            null,
            null,
            null);

        company.SetContactActive(contact.Id, false);
        company.SetLocationActive(location.Id, false);

        company.Locations.Should().ContainSingle();
        company.Locations.Single().IsActive.Should().BeFalse();
        company.Contacts.Should().ContainSingle();
        company.Contacts.Single().IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reject_Should_Keep_Company_Private_And_Require_A_Reason()
    {
        Company company = Company.CreatePrivate(Guid.NewGuid(), "Acme", null, null, null, []);

        Action action = () => company.Reject(Guid.NewGuid(), DateTime.UtcNow, "");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReviewReasonRequired);

        company.Reject(Guid.NewGuid(), DateTime.UtcNow, "Insufficient information.");
        company.Visibility.Should().Be(CompanyVisibility.Private);
        company.ReviewReason.Should().Be("Insufficient information.");
    }
}
