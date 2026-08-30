using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.CompanyContacts;

public static class GetSelectableCompanyContacts
{
    internal sealed record Query(Guid? CompanyId, Guid? CompanyLocationId, string? Search)
        : IQuery<Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Route,
                    async (
                        Guid? companyId,
                        Guid? companyLocationId,
                        string? search,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response> result =
                            await dispatcher.SendAsync(
                                new Query(companyId, companyLocationId, search),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetSelectableCompanyContacts")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        ApplicationsDbContext dbContext,
        IUserContext userContext)
        : IQueryHandler<Query, Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response>
    {
        public async Task<Result<Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<Domain.CompanyContactProjection> contacts = dbContext.CompanyContactProjections
                .AsNoTracking()
                .Where(contact =>
                    contact.IsActive &&
                    !contact.IsRejected &&
                    (contact.Visibility == JobWize.Modules.Companies.Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared ||
                     contact.CreatedByCandidateId == userContext.UserId));

            if (query.CompanyId is not null)
            {
                contacts = contacts.Where(contact => contact.CompanyId == query.CompanyId.Value);
            }

            if (query.CompanyLocationId is not null)
            {
                contacts = contacts.Where(contact => contact.CompanyLocationId == query.CompanyLocationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();
                contacts = contacts.Where(contact =>
                    EF.Functions.ILike(contact.Name, $"%{search}%") ||
                    (contact.RoleTitle != null && EF.Functions.ILike(contact.RoleTitle, $"%{search}%")));
            }

            List<Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Item> items = await contacts
                .OrderBy(contact => contact.Name)
                .ThenBy(contact => contact.RoleTitle)
                .Select(contact => new Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Item(
                    contact.Id,
                    contact.CompanyId,
                    contact.CompanyLocationId,
                    contact.Name,
                    contact.RoleTitle,
                    contact.Email,
                    contact.PhoneNumber))
                .ToListAsync(cancellationToken);

            return Result<Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response>.Success(
                new Contracts.Public.CompanyContacts.GetSelectableCompanyContacts.Response(items));
        }
    }
}
