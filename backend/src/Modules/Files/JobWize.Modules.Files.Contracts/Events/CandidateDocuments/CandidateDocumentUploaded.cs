using JobWize.Shared.Contracts.Application.Events;

namespace JobWize.Modules.Files.Contracts.Events.FileAssets;

public sealed record FileAssetUploaded(Guid DocumentId, Guid CandidateId) : IIntegrationEvent;
