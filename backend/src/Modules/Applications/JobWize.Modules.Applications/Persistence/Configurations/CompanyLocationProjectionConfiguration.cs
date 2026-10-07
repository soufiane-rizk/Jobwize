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
        builder.Property(item => item.Id)
            .ValueGeneratedNever();
        builder.Property(item => item.Label)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(item => item.Visibility)
            .HasConversion<string>()
            .HasDefaultValue(JobWize.Modules.Companies.Contracts.Public.Companies.CompanyLocationVisibility.Shared)
            .IsRequired();
        builder.HasIndex(item => new
        {
            item.CompanyId,
            item.IsActive,
            item.Visibility,
            item.CreatedByCandidateId
        });
    }
}
