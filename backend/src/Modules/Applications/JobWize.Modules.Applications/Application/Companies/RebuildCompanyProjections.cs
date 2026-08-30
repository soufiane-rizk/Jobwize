using JobWize.Modules.Applications.Contracts.Public.Companies;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.Companies;

public static class RebuildCompanyProjections
{
    internal sealed record Command : ICommand<int>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Companies.RebuildCompanyProjections.Route,
                    async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<int> result = await dispatcher.SendAsync(
                            new Command(),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.SuperAdmin)
                .WithName("RebuildCompanyProjections")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        ApplicationsDbContext dbContext,
        IDispatcher dispatcher) : ICommandHandler<Command, int>
    {
        public async Task<Result<int>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            IReadOnlyList<JobWize.Modules.Companies.Contracts.Internal.Companies.GetCompanyProjection.Response> companies =
                await dispatcher.SendModuleQueryAsync(
                    new JobWize.Modules.Companies.Contracts.Internal.Companies.GetAllCompanyProjections.Query(),
                    cancellationToken);

            Guid[] companyIds = companies
                .Select(company => company.Id)
                .ToArray();

            foreach (JobWize.Modules.Companies.Contracts.Internal.Companies.GetCompanyProjection.Response company in companies)
            {
                CompanyProjection? projection = await dbContext.CompanyProjections
                    .Include(item => item.Locations)
                    .SingleOrDefaultAsync(item => item.Id == company.Id, cancellationToken);

                if (projection is null)
                {
                    projection = CompanyProjection.CreateOrUpdate(
                        company.Id,
                        company.Name,
                        company.Visibility,
                        company.CreatedByCandidateId,
                        true);

                    dbContext.CompanyProjections.Add(projection);
                }
                else
                {
                    projection.Update(
                        company.Name,
                        company.Visibility,
                        company.CreatedByCandidateId,
                        true);
                }

                projection.SynchronizeLocations(
                    company.Locations.Select(location => (
                        location.Id,
                        location.Label,
                        location.Visibility,
                        location.CreatedByCandidateId,
                        location.IsActive)));

                Guid[] contactIds = company.Contacts.Select(contact => contact.Id).ToArray();
                List<CompanyContactProjection> removedContacts = await dbContext.CompanyContactProjections
                    .Where(contact => contact.CompanyId == company.Id && !contactIds.Contains(contact.Id))
                    .ToListAsync(cancellationToken);

                foreach (CompanyContactProjection contact in removedContacts)
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

                foreach (JobWize.Modules.Companies.Contracts.Internal.Companies.GetCompanyProjection.Contact contact in company.Contacts)
                {
                    CompanyContactProjection? contactProjection = await dbContext.CompanyContactProjections
                        .SingleOrDefaultAsync(item => item.Id == contact.Id, cancellationToken);

                    if (contactProjection is null)
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
                    }
                    else
                    {
                        contactProjection.Update(
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

            List<CompanyProjection> removedCompanyProjections = await dbContext.CompanyProjections
                .Include(item => item.Locations)
                .Where(item => item.IsActive && !companyIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

            foreach (CompanyProjection projection in removedCompanyProjections)
            {
                projection.Deactivate();
            }

            List<CompanyContactProjection> contactsForRemovedCompanies = await dbContext.CompanyContactProjections
                .Where(contact => contact.IsActive && !companyIds.Contains(contact.CompanyId))
                .ToListAsync(cancellationToken);

            foreach (CompanyContactProjection contact in contactsForRemovedCompanies)
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

            return Result<int>.Success(companies.Count);
        }
    }
}
