using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class CompanyProjectionConfiguration : IEntityTypeConfiguration<CompanyProjection>
{
    public void Configure(EntityTypeBuilder<CompanyProjection> builder)
    {
        builder.ToTable("CompanyProjections", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Visibility).HasConversion<string>().IsRequired();
        builder.HasIndex(item => new { item.IsActive, item.Visibility, item.CreatedByCandidateId });

        builder.HasMany(item => item.Locations)
            .WithOne()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(CompanyProjection.Locations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
