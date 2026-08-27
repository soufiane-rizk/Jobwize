using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobInterviewParticipant : Entity
{
    public Guid JobInterviewId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? RoleTitle { get; private set; }

    private JobInterviewParticipant()
    {
    }

    internal static JobInterviewParticipant Create(Guid interviewId, string name, string? roleTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new JobInterviewParticipant
        {
            Id = Guid.NewGuid(),
            JobInterviewId = interviewId,
            Name = name.Trim(),
            RoleTitle = string.IsNullOrWhiteSpace(roleTitle) ? null : roleTitle.Trim()
        };
    }
}
