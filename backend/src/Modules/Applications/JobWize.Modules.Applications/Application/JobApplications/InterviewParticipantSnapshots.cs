using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Shared.Application.Results;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.JobApplications;

internal static class InterviewParticipantSnapshots
{
    internal static async Task<Result<IReadOnlyList<InterviewParticipantSnapshot>>> CreateAsync(
        ApplicationsDbContext dbContext,
        JobApplication application,
        Guid candidateId,
        IReadOnlyList<Guid> companyContactIds,
        IEnumerable<(string Name, string? RoleTitle)> manualParticipants,
        CancellationToken cancellationToken)
    {
        if (companyContactIds.Distinct().Count() != companyContactIds.Count)
        {
            return Result<IReadOnlyList<InterviewParticipantSnapshot>>.Failure(
                ApplicationsErrors.CompanyContactNotAvailable);
        }

        List<CompanyContactProjection> contacts = await dbContext.CompanyContactProjections
            .AsNoTracking()
            .Where(contact =>
                companyContactIds.Contains(contact.Id) &&
                contact.CompanyId == application.CompanyId &&
                contact.IsActive &&
                !contact.IsRejected &&
                (contact.Visibility == JobWize.Modules.Companies.Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared ||
                 contact.CreatedByCandidateId == candidateId))
            .ToListAsync(cancellationToken);

        if (contacts.Count != companyContactIds.Count ||
            contacts.Any(contact => !MatchesApplicationLocation(application.CompanyLocationId, contact.CompanyLocationId)))
        {
            return Result<IReadOnlyList<InterviewParticipantSnapshot>>.Failure(
                ApplicationsErrors.CompanyContactNotAvailable);
        }

        Guid[] locationIds = contacts
            .Where(contact => contact.CompanyLocationId is not null)
            .Select(contact => contact.CompanyLocationId!.Value)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<Guid, string> locationLabels = await dbContext.CompanyLocationProjections
            .AsNoTracking()
            .Where(location => locationIds.Contains(location.Id))
            .ToDictionaryAsync(location => location.Id, location => location.Label, cancellationToken);

        List<InterviewParticipantSnapshot> snapshots = contacts
            .Select(contact => new InterviewParticipantSnapshot(
                contact.Id,
                contact.CompanyLocationId,
                contact.CompanyLocationId is Guid locationId && locationLabels.TryGetValue(locationId, out string? label)
                    ? label
                    : null,
                contact.Name,
                contact.RoleTitle,
                contact.Email,
                contact.PhoneNumber))
            .ToList();

        snapshots.AddRange(manualParticipants.Select(participant => new InterviewParticipantSnapshot(
            null,
            null,
            null,
            participant.Name,
            participant.RoleTitle,
            null,
            null)));

        return Result<IReadOnlyList<InterviewParticipantSnapshot>>.Success(snapshots);
    }

    private static bool MatchesApplicationLocation(
        Guid? applicationLocationId,
        Guid? contactLocationId) =>
        applicationLocationId is null ||
        contactLocationId is null ||
        contactLocationId == applicationLocationId;
}
