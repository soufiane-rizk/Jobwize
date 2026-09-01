using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobApplicationReminderConfiguration : IEntityTypeConfiguration<JobApplicationReminder>
{
    public void Configure(EntityTypeBuilder<JobApplicationReminder> builder)
    {
        builder.ToTable("JobApplicationReminders", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Kind).HasConversion<string>();
        builder.Property(item => item.State).HasConversion<string>();
        builder.Property(item => item.Title).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Note).HasMaxLength(4000);
        builder.HasIndex(item => new { item.JobApplicationId, item.DueAt });
    }
}
