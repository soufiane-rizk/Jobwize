using FluentAssertions;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Companies.UnitTests.Companies;

public sealed class DomainRuleTests
{
    [Fact]
    public void CreatePrivate_Should_Require_A_Company_Name()
    {
        Action action = () => Company.CreatePrivate(
            Guid.NewGuid(),
            " ",
            null,
            null,
            null,
            []);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyNameRequired);
    }

    [Fact]
    public void CreateShared_Should_Require_A_Company_Name()
    {
        Action action = () => Company.CreateShared("", null, null, null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyNameRequired);
    }

    [Fact]
    public void CreateLocation_Should_Require_A_Country()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.AddSharedLocation(null, "Casablanca", " ", null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.LocationCountryRequired);
    }

    [Fact]
    public void UpdateLocation_Should_Require_A_Country()
    {
        Company company = Company.CreateShared("Acme", null, null, null);
        CompanyLocation location = company.AddSharedLocation(null, "Casablanca", "Morocco", null);

        Action action = () => company.UpdateLocation(location.Id, null, "Casablanca", " ", null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.LocationCountryRequired);
    }

    [Fact]
    public void AddContact_Should_Require_A_Name()
    {
        Company company = Company.CreatePrivate(Guid.NewGuid(), "Acme", null, null, null, []);

        Action action = () => company.AddPrivateContact(
            Guid.NewGuid(),
            null,
            " ",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyContactNameRequired);
    }

    [Fact]
    public void AddContact_Should_Require_A_Selectably_Owned_Or_Shared_Location()
    {
        Guid ownerId = Guid.NewGuid();
        Company company = Company.CreatePrivate(
            ownerId,
            "Acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)]);
        Guid locationId = company.Locations.Single().Id;

        Action action = () => company.AddPrivateContact(
            Guid.NewGuid(),
            locationId,
            "Recruiter",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.LocationNotSelectable);
    }

    [Fact]
    public void AddSharedContact_Should_Require_An_Active_Shared_Location()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.AddSharedContact(
            Guid.NewGuid(),
            "Recruiter",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.SharedContactRequiresActiveSharedLocation);
    }

    [Fact]
    public void ApproveContact_Should_Require_A_Shared_Company()
    {
        Guid candidateId = Guid.NewGuid();
        Company company = Company.CreatePrivate(
            candidateId,
            "Acme",
            null,
            null,
            null,
            [(null, "Casablanca", "Morocco", null)],
            [(0, "Recruiter", null, null, null)]);
        CompanyLocation location = company.Locations.Single();
        CompanyContact contact = company.Contacts.Single();

        Action action = () => company.ApproveContact(
            contact.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            null,
            location.Id,
            "Recruiter",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyMustBeSharedBeforeContactApproval);
    }

    [Fact]
    public void ApproveContact_Should_Not_Review_A_Shared_Contact_Again()
    {
        Company company = Company.CreateShared("Acme", null, null, null);
        CompanyLocation location = company.AddSharedLocation(null, "Casablanca", "Morocco", null);
        CompanyContact contact = company.AddSharedContact(location.Id, "Recruiter", null, null, null);

        Action action = () => company.ApproveContact(
            contact.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            null,
            location.Id,
            "Recruiter",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyContactCannotBeReviewedAgain);
    }

    [Fact]
    public void Approve_Should_Not_Review_A_Shared_Company_Again()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.Approve(Guid.NewGuid(), DateTime.UtcNow, null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyCannotBeReviewedAgain);
    }

    [Fact]
    public void RejectContact_Should_Require_A_Reason()
    {
        Company company = Company.CreatePrivate(
            Guid.NewGuid(),
            "Acme",
            null,
            null,
            null,
            [],
            [(null, "Recruiter", null, null, null)]);
        CompanyContact contact = company.Contacts.Single();

        Action action = () => company.RejectContact(
            contact.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            " ");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReviewReasonRequired);
    }

    [Fact]
    public void UpdateLocation_Should_Require_A_Location_From_The_Company()
    {
        Company company = Company.CreateShared("Acme", null, null, null);

        Action action = () => company.UpdateLocation(
            Guid.NewGuid(),
            null,
            "Casablanca",
            "Morocco",
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyLocationNotInCompany);
    }

    [Fact]
    public void ApproveLocation_Should_Require_A_Location_From_The_Company()
    {
        Company company = Company.CreatePrivate(Guid.NewGuid(), "Acme", null, null, null, []);

        Action action = () => company.ApproveLocation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyLocationNotInCompany);
    }

    [Fact]
    public void UpdateContact_Should_Require_A_Location_From_The_Company()
    {
        Company company = Company.CreatePrivate(
            Guid.NewGuid(),
            "Acme",
            null,
            null,
            null,
            [],
            [(null, "Recruiter", null, null, null)]);
        CompanyContact contact = company.Contacts.Single();

        Action action = () => company.UpdateContact(
            contact.Id,
            Guid.NewGuid(),
            "Recruiter",
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.LocationNotInCompany);
    }

    [Fact]
    public void RejectContact_Should_Require_A_Contact_From_The_Company()
    {
        Company company = Company.CreatePrivate(Guid.NewGuid(), "Acme", null, null, null, []);

        Action action = () => company.RejectContact(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Invalid contact.");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CompanyContactNotInCompany);
    }

    [Fact]
    public void EnsureActiveSharedContactsUseActiveLocations_Should_Reject_A_Disabled_Location()
    {
        Company company = Company.CreateShared("Acme", null, null, null);
        CompanyLocation location = company.AddSharedLocation(null, "Casablanca", "Morocco", null);
        CompanyContact contact = company.AddSharedContact(location.Id, "Recruiter", null, null, null);
        company.SetLocationActive(location.Id, false);

        Action action = () => company.EnsureActiveSharedContactsUseActiveLocations();

        contact.IsActive.Should().BeTrue();
        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.SharedContactRequiresActiveSharedLocation);
    }
}
