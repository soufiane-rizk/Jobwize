using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using JobWize.Runtime.Contracts.Notifications;

namespace JobWize.Modules.Files.Application.FileAssets;

internal sealed class BindFilesToCvSubmission(IFileAssetRepository files)
    : INotificationHandler<JobApplicationCvSubmitted>
{
    public async Task HandleAsync(
        JobApplicationCvSubmitted notification,
        CancellationToken cancellationToken)
    {
        foreach (Guid fileId in notification.FileIds.Distinct())
        {
            FileAsset? file = await files.GetByIdAsync(fileId, notification.CandidateId, cancellationToken);

            if (file is null || file.Kind != FileAssetKind.CandidateDocument)
            {
                throw new InvalidOperationException(
                    $"Candidate document {fileId} is no longer available for submission binding.");
            }

            file.BindTo(
                "JobApplicationCvSubmission",
                notification.SubmissionId,
                "SubmittedDocument",
                FileBindingAccessPolicy.OwnerOnly);
        }
    }
}
