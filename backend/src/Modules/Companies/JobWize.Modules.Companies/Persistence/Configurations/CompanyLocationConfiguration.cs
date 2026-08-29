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

        builder.Property(location => location.Id)
            .ValueGeneratedNever();

        builder.Property(location => location.Label)
            .HasMaxLength(200);

        builder.Property(location => location.City)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Country)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Address)
            .HasMaxLength(500);

        builder.Property(location => location.Visibility)
            .HasConversion<string>()
            .HasDefaultValue(Contracts.Public.Companies.CompanyLocationVisibility.Shared)
            .IsRequired();

        builder.Property(location => location.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(location => location.ReviewReason)
            .HasMaxLength(4000);

        builder.HasIndex(location => new
        {
            location.CompanyId,
            location.Visibility,
            location.CreatedByCandidateId,
            location.IsActive
        });
    }
}
