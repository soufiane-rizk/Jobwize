using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;
internal sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications", Schemas.Applications);

        builder.ConfigureDomainModelBase();

        builder.HasQueryFilter(application => application.DeletedAt == null);

        builder.Property(application => application.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(application => application.RoleTitle)
            .HasMaxLength(200);

        builder.Property(application => application.SourceUrl)
            .HasMaxLength(2048);

        builder.Property(application => application.Notes)
            .HasMaxLength(8000);

        builder.Property(application => application.Kind)
            .HasConversion<string>();

        builder.Property(application => application.Status)
            .HasConversion<string>();

        builder.HasIndex(application => new
        {
            application.CandidateId,
            application.Status
        });
    }
}
