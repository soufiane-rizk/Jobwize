using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Companies.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies", Schemas.Companies);

        builder.ConfigureDomainModelBase();

        builder.HasQueryFilter(company => company.DeletedAt == null);

        builder.Property(company => company.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(company => company.Website)
            .HasMaxLength(2048);

        builder.Property(company => company.Industry)
            .HasMaxLength(200);

        builder.Property(company => company.Description)
            .HasMaxLength(8000);

        builder.Property(company => company.Visibility)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(company => company.ReviewReason)
            .HasMaxLength(4000);

        builder.HasIndex(company => company.Name);
        builder.HasIndex(company => new { company.Visibility, company.CreatedByCandidateId });

        builder.HasMany(company => company.Locations)
            .WithOne()
            .HasForeignKey(location => location.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Company.Locations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
