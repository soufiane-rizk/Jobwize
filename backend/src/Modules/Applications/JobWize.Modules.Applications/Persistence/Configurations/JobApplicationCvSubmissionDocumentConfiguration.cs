using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobApplicationCvSubmissionDocumentConfiguration : IEntityTypeConfiguration<JobApplicationCvSubmissionDocument>
{
    public void Configure(EntityTypeBuilder<JobApplicationCvSubmissionDocument> builder)
    {
        builder.ToTable("JobApplicationCvSubmissionDocuments", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.FileId).IsRequired();
        builder.Property(item => item.FileName).HasMaxLength(260).IsRequired();
        builder.Property(item => item.ContentType).HasMaxLength(255).IsRequired();
        builder.HasIndex(item => item.JobApplicationCvSubmissionId);
    }
}
