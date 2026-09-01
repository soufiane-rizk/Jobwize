using JobWize.Shared.Errors;

namespace JobWize.Modules.Applications.Domain;

public static class DomainErrors
{
    public static readonly Error ApplicationStatusUnchanged = new(
        "Applications.Domain.ApplicationStatusUnchanged",
        "The new application status must be different from the current status.",
        ErrorType.Validation);

    public static readonly Error ApplicationStatusTransitionNotAllowed = new(
        "Applications.Domain.ApplicationStatusTransitionNotAllowed",
        "The application cannot transition to the selected status.",
        ErrorType.Validation);

    public static readonly Error AppliedOnRequired = new(
        "Applications.Domain.AppliedOnRequired",
        "An applied date is required once an application has been sent.",
        ErrorType.Validation);

    public static readonly Error NoteRequired = new(
        "Applications.Domain.NoteRequired",
        "A note cannot be empty.",
        ErrorType.Validation);

    public static readonly Error InterviewParticipantNameRequired = new(
        "Applications.Domain.InterviewParticipantNameRequired",
        "An interview participant name is required.",
        ErrorType.Validation);

    public static readonly Error CvSubmissionDocumentRequired = new(
        "Applications.Domain.CvSubmissionDocumentRequired",
        "At least one document is required for a CV submission.",
        ErrorType.Validation);

    public static readonly Error DuplicateCvSubmissionDocument = new(
        "Applications.Domain.DuplicateCvSubmissionDocument",
        "A document can only be submitted once in the same CV submission.",
        ErrorType.Validation);

    public static readonly Error InterviewDateRequired = new(
        "Applications.Domain.InterviewDateRequired",
        "An interview date is required.",
        ErrorType.Validation);

    public static readonly Error InterviewDurationMustBePositive = new(
        "Applications.Domain.InterviewDurationMustBePositive",
        "Interview duration must be greater than zero.",
        ErrorType.Validation);

    public static readonly Error InterviewCannotBeUpdated = new(
        "Applications.Domain.InterviewCannotBeUpdated",
        "Only a scheduled interview can be updated.",
        ErrorType.Validation);

    public static readonly Error InterviewCannotHaveResult = new(
        "Applications.Domain.InterviewCannotHaveResult",
        "Only a scheduled interview can have a result recorded.",
        ErrorType.Validation);

    public static readonly Error InterviewResultMustBeFinal = new(
        "Applications.Domain.InterviewResultMustBeFinal",
        "The interview result must be completed, cancelled, or postponed.",
        ErrorType.Validation);

    public static readonly Error InterviewRescheduleDateRequired = new(
        "Applications.Domain.InterviewRescheduleDateRequired",
        "A new date is required when postponing an interview.",
        ErrorType.Validation);

    public static readonly Error ApplicationMustBeSentBeforeInterview = new(
        "Applications.Domain.ApplicationMustBeSentBeforeInterview",
        "Mark the application as applied before scheduling an interview.",
        ErrorType.Validation);

    public static readonly Error CannotScheduleInterviewForCurrentStatus = new(
        "Applications.Domain.CannotScheduleInterviewForCurrentStatus",
        "An interview can only be scheduled for an applied application that is in process.",
        ErrorType.Validation);

    public static readonly Error CvSubmissionNotInApplication = new(
        "Applications.Domain.CvSubmissionNotInApplication",
        "The selected CV submission does not belong to this application.",
        ErrorType.Validation);

    public static readonly Error InterviewNotInApplication = new(
        "Applications.Domain.InterviewNotInApplication",
        "The selected interview does not belong to this application.",
        ErrorType.NotFound);

    public static readonly Error ReminderRelationInvalid = new(
        "Applications.Domain.ReminderRelationInvalid",
        "The reminder relation is invalid.",
        ErrorType.Validation);

    public static readonly Error ReminderTitleRequired = new(
        "Applications.Domain.ReminderTitleRequired",
        "A reminder title is required.",
        ErrorType.Validation);

    public static readonly Error ReminderDueAtRequired = new(
        "Applications.Domain.ReminderDueAtRequired",
        "A reminder due date is required.",
        ErrorType.Validation);

    public static readonly Error ReminderCannotChangeState = new(
        "Applications.Domain.ReminderCannotChangeState",
        "Only an open reminder can be completed or dismissed.",
        ErrorType.Validation);

    public static readonly Error ReminderStateInvalid = new(
        "Applications.Domain.ReminderStateInvalid",
        "A reminder can only be completed or dismissed.",
        ErrorType.Validation);
}
