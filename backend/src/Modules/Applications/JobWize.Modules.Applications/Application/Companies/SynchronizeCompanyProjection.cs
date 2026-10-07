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
      INotificationHandler<CompanyCatalogueUpdated>
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
    }
}
