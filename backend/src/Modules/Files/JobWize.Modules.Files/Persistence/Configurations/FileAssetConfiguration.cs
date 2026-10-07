using JobWize.Modules.Files.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Files.Persistence.Configurations;

internal sealed class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("file_assets", "files");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.FileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(document => document.StorageKey).HasMaxLength(300).IsRequired();
        builder.Property(document => document.Kind).IsRequired();
        builder.HasIndex(document => new { document.CandidateId, document.ArchivedAt });
        builder.HasMany(document => document.Bindings)
            .WithOne()
            .HasForeignKey(binding => binding.FileAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
