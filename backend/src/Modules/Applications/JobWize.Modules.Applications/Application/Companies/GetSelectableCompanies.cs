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

namespace JobWize.Modules.Applications.Application.Companies;

public static class GetSelectableCompanies
{
    internal sealed record Query(string? Search) : IQuery<Contracts.Public.Companies.GetSelectableCompanies.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.Companies.GetSelectableCompanies.Route,
                    async (
                        string? search,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Companies.GetSelectableCompanies.Response> result =
                            await dispatcher.SendAsync(
                                new Query(search),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetSelectableCompanies")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        ApplicationsDbContext dbContext,
        IUserContext userContext) : IQueryHandler<Query, Contracts.Public.Companies.GetSelectableCompanies.Response>
    {
        public async Task<Result<Contracts.Public.Companies.GetSelectableCompanies.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<Domain.CompanyProjection> companies = dbContext.CompanyProjections
                .AsNoTracking()
                .Where(item =>
                    item.IsActive &&
                    (item.Visibility == JobWize.Modules.Companies.Contracts.Public.Companies.CompanyVisibility.Shared ||
                     item.CreatedByCandidateId == userContext.UserId));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();

                companies = companies.Where(item =>
                    EF.Functions.ILike(item.Name, $"%{search}%"));
            }

            List<Contracts.Public.Companies.GetSelectableCompanies.Item> items = await companies
                .OrderBy(item => item.Name)
                .Select(item => new Contracts.Public.Companies.GetSelectableCompanies.Item(
                    item.Id,
                    item.Name,
                    dbContext.CompanyLocationProjections
                        .Where(location =>
                            location.CompanyId == item.Id &&
                            location.IsActive &&
                            (location.Visibility == JobWize.Modules.Companies.Contracts.Public.Companies.CompanyLocationVisibility.Shared ||
                             location.CreatedByCandidateId == userContext.UserId))
                        .OrderBy(location => location.Label)
                        .Select(location => new Contracts.Public.Companies.GetSelectableCompanies.Location(
                            location.Id,
                            location.Label))
                        .ToList()))
                .ToListAsync(cancellationToken);

            return Result<Contracts.Public.Companies.GetSelectableCompanies.Response>.Success(
                new Contracts.Public.Companies.GetSelectableCompanies.Response(items));
        }
    }
}
