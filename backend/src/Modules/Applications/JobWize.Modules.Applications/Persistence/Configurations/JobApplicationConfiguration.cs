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

        builder.Property(application => application.LegacyCompanyName)
            .HasColumnName("CompanyName")
            .HasMaxLength(200);

        builder.HasIndex(application => application.CompanyId);

        builder.Property(application => application.RoleTitle)
            .HasMaxLength(200);

        builder.Property(application => application.SourceUrl)
            .HasMaxLength(2048);

        builder.Property(application => application.Notes)
            .HasMaxLength(8000);

        builder.Property(application => application.LastActivityAt)
            .IsRequired();

        builder.Property(application => application.Kind)
            .HasConversion<string>();

        builder.Property(application => application.Status)
            .HasConversion<string>();

        builder.HasIndex(application => new
        {
            application.CandidateId,
            application.Status
        });

        builder.HasMany(application => application.Activities)
            .WithOne()
            .HasForeignKey(change => change.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(JobApplication.Activities))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(application => application.Interviews)
            .WithOne()
            .HasForeignKey(interview => interview.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(JobApplication.Interviews))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
