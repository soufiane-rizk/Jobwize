using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobApplicationActivityConfiguration : IEntityTypeConfiguration<JobApplicationActivity>
{
    public void Configure(EntityTypeBuilder<JobApplicationActivity> builder)
    {
        builder.ToTable("JobApplicationActivities", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(activity => activity.Id).ValueGeneratedNever();
        builder.Property(activity => activity.Type).HasConversion<string>();
        builder.Property(activity => activity.Status).HasConversion<string>();
        builder.Property(activity => activity.OccurredAt).IsRequired();
        builder.Property(activity => activity.Note).HasMaxLength(4000);
        builder.HasIndex(activity => activity.JobApplicationId);
    }
}
