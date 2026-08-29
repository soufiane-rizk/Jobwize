using FluentValidation;
using JobWize.Modules.Companies.Contracts.Events.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobWize.Modules.Companies.Application.CompanyContacts;

public static class CreateCompanyContact
{
    internal sealed record Command(
        Guid CompanyId,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber)
        : ICommand<Contracts.Public.CompanyContacts.CreateCompanyContact.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
            RuleFor(command => command.RoleTitle).MaximumLength(200);
            RuleFor(command => command.Email).MaximumLength(320).EmailAddress();
            RuleFor(command => command.PhoneNumber).MaximumLength(50);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.CompanyContacts.CreateCompanyContact.Route,
                    async (
                        Guid companyId,
                        Contracts.Public.CompanyContacts.CreateCompanyContact.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.CompanyContacts.CreateCompanyContact.Response> result =
                            await dispatcher.SendAsync(
                                new Command(
                                    companyId,
                                    request.CompanyLocationId,
                                    request.Name,
                                    request.RoleTitle,
                                    request.Email,
                                    request.PhoneNumber),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("CreateCompanyContact")
                .WithTags("Company contacts");
        }
    }

    internal sealed class Handler(
        ICompanyRepository companies,
        IUserContext userContext,
        IDispatcher dispatcher)
        : ICommandHandler<Command, Contracts.Public.CompanyContacts.CreateCompanyContact.Response>
    {
        public async Task<Result<Contracts.Public.CompanyContacts.CreateCompanyContact.Response>> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            Domain.Company? company = await companies.GetByIdAsync(
                command.CompanyId,
                cancellationToken);

            if (company is null ||
                (company.Visibility != CompanyVisibility.Shared &&
                 company.CreatedByCandidateId != userContext.UserId))
            {
                return Result<Contracts.Public.CompanyContacts.CreateCompanyContact.Response>.Failure(
                    CompaniesErrors.CompanyNotFound);
            }

            try
            {
                Domain.CompanyContact contact = company.AddPrivateContact(
                    userContext.UserId,
                    command.CompanyLocationId,
                    command.Name,
                    command.RoleTitle,
                    command.Email,
                    command.PhoneNumber);

                await companies.SaveAsync(company, cancellationToken);

                await dispatcher.PublishAsync(
                    new CompanyContactCreated(
                        company.Id,
                        contact.Id,
                        userContext.UserId),
                    cancellationToken);

                return Result<Contracts.Public.CompanyContacts.CreateCompanyContact.Response>.Success(
                    new Contracts.Public.CompanyContacts.CreateCompanyContact.Response(contact.Id));
            }
            catch (ArgumentException)
            {
                return Result<Contracts.Public.CompanyContacts.CreateCompanyContact.Response>.Failure(
                    CompaniesErrors.CompanyLocationNotFound);
            }
        }
    }
}
