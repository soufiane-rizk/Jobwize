using FluentAssertions;
using JobWize.Modules.Identity.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Identity.UnitTests.Users;

public sealed class DomainRuleTests
{
    [Fact]
    public void CreateCandidate_Should_Require_All_Required_Values()
    {
        Action action = () => User.CreateCandidate(
            " ",
            "hash",
            "Jane",
            "Doe");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.RequiredValue);
    }

    [Fact]
    public void CreateSuperAdmin_Should_Require_All_Required_Values()
    {
        Action action = () => User.CreateSuperAdmin("admin@example.com", " ");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.RequiredValue);
    }

    [Fact]
    public void CreateAdmin_Should_Require_All_Required_Values()
    {
        Action action = () => User.CreateAdmin(
            "admin@example.com",
            "hash",
            " ",
            "Admin");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.RequiredValue);
    }

    [Fact]
    public void UpdatePersonalInformation_Should_Require_All_Required_Values()
    {
        User user = User.CreateCandidate("jane@example.com", "hash", "Jane", "Doe");

        Action action = () => user.UpdatePersonalInformation(" ", "Smith");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.RequiredValue);
    }

    [Fact]
    public void ChangePassword_Should_Require_A_Password_Hash()
    {
        User user = User.CreateCandidate("jane@example.com", "hash", "Jane", "Doe");

        Action action = () => user.ChangePassword(" ");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.RequiredValue);
    }
}
