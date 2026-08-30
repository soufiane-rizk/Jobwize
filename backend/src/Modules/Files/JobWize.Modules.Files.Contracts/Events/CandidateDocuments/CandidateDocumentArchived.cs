using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Files.Contracts.Events.FileAssets;

public sealed record FileAssetArchived(Guid DocumentId, Guid CandidateId) : IIntegrationEvent;
