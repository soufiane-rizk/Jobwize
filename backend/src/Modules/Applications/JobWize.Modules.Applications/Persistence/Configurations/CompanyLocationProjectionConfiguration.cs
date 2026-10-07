using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class CompanyLocationProjectionConfiguration : IEntityTypeConfiguration<CompanyLocationProjection>
{
    public void Configure(EntityTypeBuilder<CompanyLocationProjection> builder)
    {
        builder.ToTable("CompanyLocationProjections", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Label).HasMaxLength(200).IsRequired();
        builder.HasIndex(item => new { item.CompanyId, item.IsActive });
    }
}
