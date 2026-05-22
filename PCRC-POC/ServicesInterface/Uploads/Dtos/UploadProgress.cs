using PCRC.Model.Uploads;

namespace PCRC.ServicesInterface.Uploads.Dtos;

public sealed record UploadProgress(
    Guid UploadExternalId,
    UploadSourceType SourceType,
    UploadStatus Status,
    int? TotalCount,
    int ProcessedCount,
    int DedupedCount,
    int FailedCount,
    int AwaitingMappingCount,
    DateTime CreatedAt,
    DateTime? CompletedAt);
