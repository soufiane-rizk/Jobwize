using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Companies.Persistence.Configurations;

internal sealed class CompanyContactConfiguration : IEntityTypeConfiguration<CompanyContact>
{
    public void Configure(EntityTypeBuilder<CompanyContact> builder)
    {
        builder.ToTable("CompanyContacts", Schemas.Companies);

        builder.ConfigureEntityBase();

        builder.Property(contact => contact.Id)
            .ValueGeneratedNever();

        builder.Property(contact => contact.CreatedAt)
            .IsRequired();

        builder.Property(contact => contact.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(contact => contact.RoleTitle)
            .HasMaxLength(200);

        builder.Property(contact => contact.Email)
            .HasMaxLength(320);

        builder.Property(contact => contact.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(contact => contact.Visibility)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(contact => contact.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(contact => contact.ReviewReason)
            .HasMaxLength(4000);

        builder.HasIndex(contact => new
        {
            contact.CompanyId,
            contact.Visibility,
            contact.CreatedByCandidateId
        });

        builder.HasIndex(contact => contact.CompanyLocationId);
    }
}
