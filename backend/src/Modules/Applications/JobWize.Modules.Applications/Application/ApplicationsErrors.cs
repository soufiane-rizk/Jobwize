using JobWize.Shared.Application.Results;

namespace JobWize.Modules.Applications.Application;

public static class ApplicationsErrors
{
    public static readonly Error JobApplicationNotFound = new(
        "Applications.JobApplicationNotFound",
        "The requested job application was not found.",
        ErrorType.NotFound);

    public static readonly Error ApplicationMustBeSentBeforeInterview = new(
        "Applications.ApplicationMustBeSentBeforeInterview",
        "Mark the application as applied before scheduling an interview.",
        ErrorType.Validation);

    public static readonly Error CannotScheduleInterviewForCurrentStatus = new(
        "Applications.CannotScheduleInterviewForCurrentStatus",
        "An interview can only be scheduled for an applied application that is in process.",
        ErrorType.Validation);

    public static readonly Error InterviewNotFound = new(
        "Applications.InterviewNotFound",
        "The requested interview was not found.",
        ErrorType.NotFound);

    public static readonly Error InterviewResultMustBeFinal = new(
        "Applications.InterviewResultMustBeFinal",
        "Select completed, cancelled, or postponed as the interview result.",
        ErrorType.Validation);
}
