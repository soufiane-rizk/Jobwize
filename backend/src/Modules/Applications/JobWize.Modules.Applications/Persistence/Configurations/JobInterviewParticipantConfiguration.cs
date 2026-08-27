using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobWize.Modules.Applications.Persistence.Configurations;

internal sealed class JobInterviewParticipantConfiguration : IEntityTypeConfiguration<JobInterviewParticipant>
{
    public void Configure(EntityTypeBuilder<JobInterviewParticipant> builder)
    {
        builder.ToTable("JobInterviewParticipants", Schemas.Applications);
        builder.ConfigureEntityBase();
        builder.Property(participant => participant.Id).ValueGeneratedNever();
        builder.Property(participant => participant.Name).HasMaxLength(200).IsRequired();
        builder.Property(participant => participant.RoleTitle).HasMaxLength(200);
    }
}
