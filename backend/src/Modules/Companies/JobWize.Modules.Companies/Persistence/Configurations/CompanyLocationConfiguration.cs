using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Companies.Persistence.Configurations;

internal sealed class CompanyLocationConfiguration : IEntityTypeConfiguration<CompanyLocation>
{
    public void Configure(EntityTypeBuilder<CompanyLocation> builder)
    {
        builder.ToTable("CompanyLocations", Schemas.Companies);

        builder.ConfigureEntityBase();

        builder.Property(location => location.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.City)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Country)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Address)
            .HasMaxLength(500);

        builder.HasIndex(location => location.CompanyId);
    }
}
