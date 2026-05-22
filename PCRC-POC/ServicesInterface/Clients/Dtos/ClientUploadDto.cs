using PCRC.Model.Uploads;

namespace PCRC.ServicesInterface.Clients.Dtos;

public sealed record ClientUploadDto(
    long Id,
    Guid ExternalId,
    long ClientId,
    UploadSourceType SourceType,
    UploadStatus Status,
    int? TotalCount,
    int ProcessedCount,
    int DedupedCount,
    int FailedCount,
    int AwaitingMappingCount,
    DateTime CreatedAt,
    DateTime? CompletedAt);
