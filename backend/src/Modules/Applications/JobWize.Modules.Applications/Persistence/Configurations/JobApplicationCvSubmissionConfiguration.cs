using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobApplicationCvSubmissionConfiguration : IEntityTypeConfiguration<JobApplicationCvSubmission>
{
    public void Configure(EntityTypeBuilder<JobApplicationCvSubmission> builder)
    {
        builder.ToTable("JobApplicationCvSubmissions", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Method).HasConversion<string>().IsRequired();
        builder.Property(item => item.SentAt).IsRequired();
        builder.Property(item => item.Notes).HasMaxLength(4000);
        builder.Property(item => item.ContactName).HasMaxLength(200);
        builder.Property(item => item.ContactRoleTitle).HasMaxLength(200);
        builder.Property(item => item.ContactEmail).HasMaxLength(320);
        builder.Property(item => item.ContactPhoneNumber).HasMaxLength(50);
        builder.HasIndex(item => item.JobApplicationId);

        builder.HasMany(item => item.Documents)
            .WithOne()
            .HasForeignKey(item => item.JobApplicationCvSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(JobApplicationCvSubmission.Documents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
