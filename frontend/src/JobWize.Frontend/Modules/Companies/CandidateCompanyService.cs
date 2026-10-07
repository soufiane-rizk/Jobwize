using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

namespace JobWize.Frontend.Modules.Companies;

public sealed class CandidateCompanyService(
    IHttpClientFactory httpClientFactory,
    JobWizeAuthenticationStateProvider authenticationStateProvider)
    : ApiService(httpClientFactory, authenticationStateProvider)
{
    public Task<Result<GetCompany.Response>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetCompany.Request, GetCompany.Response>(
            GetCompany.Route,
            new GetCompany.Request(id),
            cancellationToken);
    }

    public Task<Result<CreatePrivateCompany.Response>> CreatePrivateAsync(
        CreatePrivateCompany.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreatePrivateCompany.Request, CreatePrivateCompany.Response>(
            CreatePrivateCompany.Route,
            request,
            cancellationToken);
    }

    public Task<Result<GetCompanyContacts.Response>> GetContactsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return GetAsync<GetCompanyContacts.Request, GetCompanyContacts.Response>(GetCompanyContacts.Route, new(companyId), cancellationToken);
    }

    public Task<Result<CreateCompanyContact.Response>> CreateContactAsync(CreateCompanyContact.Request request, CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateCompanyContact.Request, CreateCompanyContact.Response>(CreateCompanyContact.Route, request, cancellationToken);
    }
}
