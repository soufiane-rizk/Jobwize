namespace JobWize.Frontend.Modules.Companies;

internal static class CompaniesRoutes
{
    public const string Detail = "/companies/{Id:guid}";
    public const string ReviewQueue = "/companies/review";
    public const string ContactReviewQueue = "/company-contacts/review";
    public const string CatalogueManagement = "/companies/manage";
}
