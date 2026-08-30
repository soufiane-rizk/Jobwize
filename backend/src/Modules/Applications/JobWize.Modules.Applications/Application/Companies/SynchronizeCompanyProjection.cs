using JobWize.Modules.Companies.Contracts.Events.Companies;
using JobWize.Modules.Companies.Contracts.Internal.Companies;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.Companies;

internal sealed class SynchronizeCompanyProjection(
    ApplicationsDbContext dbContext,
    IDispatcher dispatcher)
    : INotificationHandler<CompanyCreated>,
      INotificationHandler<CompanyPromotedToShared>,
      INotificationHandler<CompanyCatalogueUpdated>,
      INotificationHandler<CompanyContactCreated>,
      INotificationHandler<CompanyContactReviewed>
{
    public Task HandleAsync(CompanyCreated notification, CancellationToken cancellationToken)
    {
        return SynchronizeAsync(notification.CompanyId, cancellationToken);
    }

    public Task HandleAsync(CompanyPromotedToShared notification, CancellationToken cancellationToken)
    {
        return SynchronizeAsync(notification.CompanyId, cancellationToken);
    }

    public Task HandleAsync(CompanyCatalogueUpdated notification, CancellationToken cancellationToken)
    {
        return SynchronizeAsync(notification.CompanyId, cancellationToken);
    }

    public Task HandleAsync(CompanyContactCreated notification, CancellationToken cancellationToken)
    {
        return SynchronizeAsync(notification.CompanyId, cancellationToken);
    }

    public Task HandleAsync(CompanyContactReviewed notification, CancellationToken cancellationToken)
    {
        return SynchronizeAsync(notification.CompanyId, cancellationToken);
    }

    private async Task SynchronizeAsync(Guid companyId, CancellationToken cancellationToken)
    {
        GetCompanyProjection.Response source = await dispatcher.SendModuleQueryAsync(
            new GetCompanyProjection.Query(companyId),
            cancellationToken);

        CompanyProjection? projection = await dbContext.CompanyProjections
            .Include(item => item.Locations)
            .SingleOrDefaultAsync(item => item.Id == companyId, cancellationToken);
        if (projection is null)
        {
            projection = CompanyProjection.CreateOrUpdate(
                source.Id,
                source.Name,
                source.Visibility,
                source.CreatedByCandidateId,
                true);

            dbContext.CompanyProjections.Add(projection);
        }
        else
        {
            projection.Update(source.Name, source.Visibility, source.CreatedByCandidateId, true);
        }

        projection.SynchronizeLocations(source.Locations.Select(location => (
            location.Id,
            location.Label,
            location.Visibility,
            location.CreatedByCandidateId,
            location.IsActive)));

        Guid[] sourceContactIds = source.Contacts.Select(contact => contact.Id).ToArray();

        List<CompanyContactProjection> missingSourceContacts = await dbContext.CompanyContactProjections
            .Where(contact => contact.CompanyId == companyId && !sourceContactIds.Contains(contact.Id))
            .ToListAsync(cancellationToken);

        foreach (CompanyContactProjection contact in missingSourceContacts)
        {
            contact.Update(
                contact.CompanyLocationId,
                contact.Name,
                contact.RoleTitle,
                contact.Email,
                contact.PhoneNumber,
                contact.Visibility,
                contact.CreatedByCandidateId,
                false,
                contact.IsRejected);
        }

        foreach (GetCompanyProjection.Contact contact in source.Contacts)
        {
            CompanyContactProjection? existing = await dbContext.CompanyContactProjections
                .SingleOrDefaultAsync(item => item.Id == contact.Id, cancellationToken);

            if (existing is null)
            {
                dbContext.CompanyContactProjections.Add(CompanyContactProjection.Create(
                    contact.Id,
                    contact.CompanyId,
                    contact.CompanyLocationId,
                    contact.Name,
                    contact.RoleTitle,
                    contact.Email,
                    contact.PhoneNumber,
                    contact.Visibility,
                    contact.CreatedByCandidateId,
                    contact.IsActive,
                    contact.IsRejected));
                continue;
            }

            existing.Update(
                contact.CompanyLocationId,
                contact.Name,
                contact.RoleTitle,
                contact.Email,
                contact.PhoneNumber,
                contact.Visibility,
                contact.CreatedByCandidateId,
                contact.IsActive,
                contact.IsRejected);
        }
    }
}
