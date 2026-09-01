using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Applications.Application;

public static class ApplicationsErrors
{
    public static readonly Error JobApplicationNotFound = new(
        "Applications.JobApplicationNotFound",
        "The requested job application was not found.",
        ErrorType.NotFound);

    public static readonly Error InterviewNotFound = new(
        "Applications.InterviewNotFound",
        "The requested interview was not found.",
        ErrorType.NotFound);

    public static readonly Error CompanyNotAvailable = new(
        "Applications.CompanyNotAvailable",
        "The selected company is not available.",
        ErrorType.Validation);

    public static readonly Error CompanyLocationNotAvailable = new(
        "Applications.CompanyLocationNotAvailable",
        "The selected company location is not available.",
        ErrorType.Validation);

    public static readonly Error CompanyContactNotAvailable = new(
        "Applications.CompanyContactNotAvailable",
        "The selected company contact is not available.",
        ErrorType.Validation);

    public static readonly Error CandidateDocumentNotAvailable = new(
        "Applications.CandidateDocumentNotAvailable",
        "One or more selected documents are not available.",
        ErrorType.Validation);

    public static readonly Error ReminderNotFound = new(
        "Applications.ReminderNotFound",
        "The requested reminder was not found.",
        ErrorType.NotFound);

}
