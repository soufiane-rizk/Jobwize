using FluentValidation;
using JobWize.Modules.Companies.Contracts.Events.Companies;
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

public static class CreatePrivateCompany
{
    internal sealed record Location(string? Label, string City, string Country, string? Address);
    internal sealed record Contact(int? LocationIndex, string Name, string? RoleTitle, string? Email, string? PhoneNumber);

    internal sealed record Command(
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        IReadOnlyList<Location> Locations,
        IReadOnlyList<Contact> Contacts) : ICommand<Contracts.Public.Companies.CreatePrivateCompany.Response>;

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
            RuleForEach(command => command.Locations).SetValidator(new LocationValidator());
            RuleForEach(command => command.Contacts).SetValidator(new ContactValidator());
            RuleForEach(command => command.Contacts)
                .Must((command, contact) =>
                    contact.LocationIndex is null ||
                    (contact.LocationIndex >= 0 && contact.LocationIndex < command.Locations.Count))
                .WithMessage("A contact location must refer to a submitted location.");
        }
    }

    internal sealed class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator()
        {
            RuleFor(contact => contact.Name).NotEmpty().MaximumLength(200);
            RuleFor(contact => contact.RoleTitle).MaximumLength(200);
            RuleFor(contact => contact.Email).MaximumLength(320).EmailAddress();
            RuleFor(contact => contact.PhoneNumber).MaximumLength(50);
        }
    }

    internal sealed class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator()
        {
            RuleFor(location => location.Label).MaximumLength(200);
            RuleFor(location => location.City).NotEmpty().MaximumLength(200);
            RuleFor(location => location.Country).NotEmpty().MaximumLength(200);
            RuleFor(location => location.Address).MaximumLength(500);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Companies.CreatePrivateCompany.Route,
                    async (
                        Contracts.Public.Companies.CreatePrivateCompany.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new Command(
                            request.Name,
                            request.Website,
                            request.Industry,
                            request.Description,
                            request.Locations
                                .Select(location => new Location(location.Label, location.City, location.Country, location.Address)).ToList(),
                            request.Contacts.Select(contact => new Contact(contact.LocationIndex, contact.Name, contact.RoleTitle, contact.Email, contact.PhoneNumber)).ToList());

                        Result<Contracts.Public.Companies.CreatePrivateCompany.Response> result =
                            await dispatcher.SendAsync(command, cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("CreatePrivateCompany")
                .WithTags("Companies");
        }
    }

    internal sealed class Handler(
        ICompanyRepository companies,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, Contracts.Public.Companies.CreatePrivateCompany.Response>
    {
        public async Task<Result<Contracts.Public.Companies.CreatePrivateCompany.Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.Company company = Domain.Company.CreatePrivate(
                userContext.UserId,
                command.Name,
                command.Website,
                command.Industry,
                command.Description,
                command.Locations.Select(location => (location.Label, location.City, location.Country, location.Address)),
                command.Contacts.Select(contact => (contact.LocationIndex, contact.Name, contact.RoleTitle, contact.Email, contact.PhoneNumber)));

            await companies.SaveAsync(company, cancellationToken);

            await dispatcher.PublishAsync(new CompanyCreated(company.Id, userContext.UserId), cancellationToken);

            return Result<Contracts.Public.Companies.CreatePrivateCompany.Response>.Success(new(company.Id));
        }
    }
}
