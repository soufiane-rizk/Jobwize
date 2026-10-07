using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class CompanyContactProjectionConfiguration : IEntityTypeConfiguration<CompanyContactProjection>
{
    public void Configure(EntityTypeBuilder<CompanyContactProjection> builder)
    {
        builder.ToTable("CompanyContactProjections", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.RoleTitle).HasMaxLength(200);
        builder.Property(item => item.Email).HasMaxLength(320);
        builder.Property(item => item.PhoneNumber).HasMaxLength(50);
        builder.Property(item => item.Visibility).HasConversion<string>().IsRequired();
        builder.HasIndex(item => new
        {
            item.CompanyId,
            item.CompanyLocationId,
            item.IsActive,
            item.IsRejected,
            item.Visibility,
            item.CreatedByCandidateId
        }).HasDatabaseName("IX_CompanyContactProjections_Selectability");

        builder.HasOne<CompanyProjection>()
            .WithMany()
            .HasForeignKey(item => item.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
