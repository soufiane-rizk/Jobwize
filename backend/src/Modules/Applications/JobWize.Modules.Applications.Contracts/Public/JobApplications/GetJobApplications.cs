namespace JobWize.Modules.Applications.Contracts.Public.JobApplications;

public static class GetJobApplications
{
    public const string Route = "/api/applications";

    public sealed record Request();

    public sealed record Item(
        Guid Id,
        string CompanyName,
        string? RoleTitle,
        ApplicationKind Kind,
        ApplicationStatus Status,
        DateOnly? AppliedOn,
        DateTime CreatedAt);

    public sealed record Response(IReadOnlyList<Item> Applications);
}
