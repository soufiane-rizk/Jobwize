using FluentValidation;
using JobWize.Modules.Companies.Contracts.Events.Companies;
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

namespace JobWize.Modules.Companies.Application.Companies;

public static class UpdateCompanyCatalogue
{
    internal sealed record Command(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        IReadOnlyList<Contracts.Public.Companies.UpdateCompanyCatalogue.Location> Locations,
        IReadOnlyList<Contracts.Public.Companies.UpdateCompanyCatalogue.Contact> Contacts) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Website)
                .MaximumLength(2048)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website must be an absolute URL.");
            RuleFor(command => command.Industry).MaximumLength(200);
            RuleFor(command => command.Description).MaximumLength(8000);
            RuleForEach(command => command.Locations).ChildRules(location =>
            {
                location.RuleFor(item => item.Label).MaximumLength(200);
                location.RuleFor(item => item.City).NotEmpty().MaximumLength(200);
                location.RuleFor(item => item.Country).NotEmpty().MaximumLength(200);
                location.RuleFor(item => item.Address).MaximumLength(500);
            });
            RuleForEach(command => command.Contacts).ChildRules(contact =>
            {
                contact.RuleFor(item => item.Name).NotEmpty().MaximumLength(200);
                contact.RuleFor(item => item.RoleTitle).MaximumLength(200);
                contact.RuleFor(item => item.Email).MaximumLength(320).EmailAddress();
                contact.RuleFor(item => item.PhoneNumber).MaximumLength(50);
            });
            RuleForEach(command => command.Contacts)
                .Must((command, contact) =>
                    contact.LocationIndex is null ||
                    (contact.LocationIndex >= 0 && contact.LocationIndex < command.Locations.Count))
                .WithMessage("A contact location must refer to a submitted location.");
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    Contracts.Public.Companies.UpdateCompanyCatalogue.Route,
                    async (
                        Guid id,
                        Contracts.Public.Companies.UpdateCompanyCatalogue.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(
                                id,
                                request.Name,
                                request.Website,
                                request.Industry,
                                request.Description,
                                request.Locations,
                                request.Contacts),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
                .WithName("UpdateCompanyCatalogue")
                .WithTags("Companies");
        }
    }

    internal sealed class Handler(
        ICompanyRepository companies,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.Company? company = await companies.GetByIdAsync(command.Id, cancellationToken);

            if (company is null || company.Visibility != CompanyVisibility.Shared)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyNotFound);
            }

            foreach (Contracts.Public.Companies.UpdateCompanyCatalogue.Location location in command.Locations)
            {
                if (location.Id is not null &&
                    !company.Locations.Any(item =>
                        item.Id == location.Id.Value &&
                        item.Visibility == CompanyLocationVisibility.Shared))
                {
                    return Result<bool>.Failure(CompaniesErrors.CompanyLocationNotFound);
                }
            }

            foreach (Contracts.Public.Companies.UpdateCompanyCatalogue.Contact contact in command.Contacts)
            {
                if (contact.Id is not null &&
                    !company.Contacts.Any(item =>
                        item.Id == contact.Id.Value &&
                        item.Visibility == Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared))
                {
                    return Result<bool>.Failure(CompaniesErrors.CompanyContactNotFound);
                }
            }

            company.ReplaceBasicInformation(
                command.Name,
                command.Website,
                command.Industry,
                command.Description);

            var resolvedLocations = new List<Domain.CompanyLocation>();

            foreach (Contracts.Public.Companies.UpdateCompanyCatalogue.Location location in command.Locations)
            {
                Domain.CompanyLocation resolved;

                if (location.Id is null)
                {
                    resolved = company.AddSharedLocation(
                        location.Label,
                        location.City,
                        location.Country,
                        location.Address);
                }
                else
                {
                    company.UpdateLocation(
                        location.Id.Value,
                        location.Label,
                        location.City,
                        location.Country,
                        location.Address);
                    company.SetLocationActive(location.Id.Value, location.IsActive);
                    resolved = company.Locations.Single(item => item.Id == location.Id.Value);
                }

                resolvedLocations.Add(resolved);
            }

            foreach (Contracts.Public.Companies.UpdateCompanyCatalogue.Contact contact in command.Contacts)
            {
                Guid? locationId = contact.LocationIndex is null
                    ? null
                    : resolvedLocations[contact.LocationIndex.Value].Id;

                if (contact.Id is null)
                {
                    company.AddSharedContact(
                        locationId,
                        contact.Name,
                        contact.RoleTitle,
                        contact.Email,
                        contact.PhoneNumber);
                }
                else
                {
                    company.UpdateContact(
                        contact.Id.Value,
                        locationId,
                        contact.Name,
                        contact.RoleTitle,
                        contact.Email,
                        contact.PhoneNumber);
                    company.SetContactActive(contact.Id.Value, contact.IsActive);
                }
            }

            company.EnsureActiveSharedContactsUseActiveLocations();

            await companies.SaveAsync(company, cancellationToken);
            await dispatcher.PublishAsync(
                new CompanyCatalogueUpdated(company.Id, userContext.UserId),
                cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
