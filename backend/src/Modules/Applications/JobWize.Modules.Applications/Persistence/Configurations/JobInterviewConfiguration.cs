using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobInterviewConfiguration : IEntityTypeConfiguration<JobInterview>
{
    public void Configure(EntityTypeBuilder<JobInterview> builder)
    {
        builder.ToTable("JobInterviews", Schemas.Applications);
        builder.ConfigureDomainModelBase();
        builder.Property(interview => interview.Id).ValueGeneratedNever();
        builder.Property(interview => interview.Type).HasConversion<string>();
        builder.Property(interview => interview.State).HasConversion<string>();
        builder.Property(interview => interview.Format).HasConversion<string>();
        builder.Property(interview => interview.Location).HasMaxLength(2048);
        builder.Property(interview => interview.PreparationNotes).HasMaxLength(4000);
        builder.HasMany(interview => interview.Participants)
            .WithOne()
            .HasForeignKey(participant => participant.JobInterviewId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(JobInterview.Participants))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
