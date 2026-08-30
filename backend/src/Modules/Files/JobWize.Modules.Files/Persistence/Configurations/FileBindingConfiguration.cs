using JobWize.Modules.Files.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Files.Persistence.Configurations;

internal sealed class FileBindingConfiguration : IEntityTypeConfiguration<FileBinding>
{
    public void Configure(EntityTypeBuilder<FileBinding> builder)
    {
        builder.ToTable("file_bindings", "files");
        builder.HasKey(binding => binding.Id);
        builder.Property(binding => binding.ResourceType).HasMaxLength(100).IsRequired();
        builder.Property(binding => binding.Usage).HasMaxLength(100).IsRequired();
        builder.Property(binding => binding.AccessPolicy).IsRequired();
        builder.HasIndex(binding => new { binding.FileAssetId, binding.ReleasedAt });
        builder.HasIndex(binding => new { binding.ResourceType, binding.ResourceId, binding.Usage, binding.ReleasedAt });
    }
}
