using FluentAssertions;
using JobWize.Modules.Files.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class DomainRuleTests
{
    [Fact]
    public void Create_Should_Require_A_File_Name()
    {
        Action action = () => FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CandidateDocument,
            " ",
            "application/pdf",
            10,
            "candidate/document");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.FileNameRequired);
    }

    [Fact]
    public void Create_Should_Require_A_Content_Type()
    {
        Action action = () => FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CandidateDocument,
            "cv.pdf",
            " ",
            10,
            "candidate/document");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ContentTypeRequired);
    }

    [Fact]
    public void Create_Should_Require_A_Storage_Key()
    {
        Action action = () => FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CandidateDocument,
            "cv.pdf",
            "application/pdf",
            10,
            " ");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.StorageKeyRequired);
    }

    [Fact]
    public void BindTo_Should_Require_A_Resource_Type()
    {
        FileAsset file = CreateFile();

        Action action = () => file.BindTo(
            " ",
            Guid.NewGuid(),
            "Logo",
            FileBindingAccessPolicy.ResourceViewers);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.BindingResourceTypeRequired);
    }

    [Fact]
    public void BindTo_Should_Require_A_Usage()
    {
        FileAsset file = CreateFile();

        Action action = () => file.BindTo(
            "Company",
            Guid.NewGuid(),
            " ",
            FileBindingAccessPolicy.ResourceViewers);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.BindingUsageRequired);
    }

    [Fact]
    public void BindTo_Should_Reject_An_Archived_File()
    {
        FileAsset file = CreateFile();
        file.Archive(DateTime.UtcNow);

        Action action = () => file.BindTo(
            "Company",
            Guid.NewGuid(),
            "Logo",
            FileBindingAccessPolicy.ResourceViewers);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ArchivedFileCannotBeBound);
    }

    private static FileAsset CreateFile()
    {
        return FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CompanyLogo,
            "logo.png",
            "image/png",
            10,
            "company/logo");
    }
}
