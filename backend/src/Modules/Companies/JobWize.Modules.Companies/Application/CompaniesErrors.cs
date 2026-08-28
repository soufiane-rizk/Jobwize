using JobWize.Shared.Application.Results;

namespace JobWize.Modules.Companies.Application;

public static class CompaniesErrors
{
    public static readonly Error CompanyNotFound = new("Companies.CompanyNotFound", "The requested company was not found.", ErrorType.NotFound);
}
