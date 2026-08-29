using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

namespace JobWize.Frontend.Modules.Companies;

public sealed class CompanyModerationService(
    IHttpClientFactory httpClientFactory,
    JobWizeAuthenticationStateProvider authenticationStateProvider)
    : ApiService(httpClientFactory, authenticationStateProvider)
{
    public Task<Result<GetCompaniesForReview.Response>> GetForReviewAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<object, GetCompaniesForReview.Response>(
            GetCompaniesForReview.Route,
            new object(),
            cancellationToken);
    }

    public Task<Result<bool>> ReviewAsync(
        ReviewCompany.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<ReviewCompany.Request, bool>(
            ReviewCompany.Route,
            request,
            cancellationToken);
    }

    public Task<Result<GetCompanies.Response>> GetCatalogueAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetCompanies.Request, GetCompanies.Response>(
            GetCompanies.Route,
            new GetCompanies.Request(search),
            cancellationToken);
    }

    public Task<Result<GetCompanyForManagement.Response>> GetForManagementAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetCompanyForManagement.Request, GetCompanyForManagement.Response>(
            GetCompanyForManagement.Route,
            new GetCompanyForManagement.Request(companyId),
            cancellationToken);
    }

    public Task<Result<bool>> UpdateCatalogueAsync(
        UpdateCompanyCatalogue.Request request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<UpdateCompanyCatalogue.Request, bool>(
            UpdateCompanyCatalogue.Route,
            request,
            cancellationToken);
    }

    public Task<Result<GetCompanyContactsForReview.Response>> GetContactsForReviewAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<object, GetCompanyContactsForReview.Response>(
            GetCompanyContactsForReview.Route,
            new(),
            cancellationToken);

    public Task<Result<bool>> ReviewContactAsync(
        ReviewCompanyContact.Request request,
        CancellationToken cancellationToken = default) =>
        PostAsync<ReviewCompanyContact.Request, bool>(
            ReviewCompanyContact.Route,
            request,
            cancellationToken);
}
