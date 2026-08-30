using FluentAssertions;
using JobWize.Modules.Files.Domain;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class FileAssetTests
{
    [Fact]
    public void Create_Should_Reject_Empty_File()
    {
        Action act = () => FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CandidateDocument,
            "cv.pdf",
            "application/pdf",
            0,
            "candidate/document");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Archive_Should_Preserve_Metadata_And_Prevent_A_Second_Archive()
    {
        FileAsset document = FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CandidateDocument,
            "cv.pdf",
            "application/pdf",
            20,
            "candidate/document");

        document.Archive(new DateTime(2026, 8, 30, 10, 0, 0, DateTimeKind.Utc));

        document.IsArchived.Should().BeTrue();
        document.FileName.Should().Be("cv.pdf");
        document.StorageKey.Should().Be("candidate/document");
        Action act = () => document.Archive(DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BindTo_Should_Create_Only_One_Active_Binding_For_The_Same_Usage()
    {
        FileAsset file = FileAsset.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FileAssetKind.CompanyLogo,
            "logo.png",
            "image/png",
            20,
            "candidate/file");

        Guid companyId = Guid.NewGuid();
        file.BindTo("Company", companyId, "Logo", FileBindingAccessPolicy.ResourceViewers);
        file.BindTo("Company", companyId, "Logo", FileBindingAccessPolicy.ResourceViewers);

        file.HasActiveBindings.Should().BeTrue();
        file.Bindings.Should().ContainSingle(binding =>
            binding.ResourceType == "Company" &&
            binding.ResourceId == companyId &&
            binding.Usage == "Logo");
    }
}
