using PCRC.Model.Documents;

namespace PCRC.ServicesInterface.Documents.Dtos;

public sealed record DocumentDto(
    long Id,
    Guid ExternalId,
    DocumentType DocumentType,
    long? UploadId,
    long ClientId,
    Guid ClientExternalId,
    string? OriginalFileName,
    string? ContentType,
    long? SizeBytes,
    DocumentStatus Status,
    string? ErrorMessage,
    DateTime UploadedAt,
    DateTime? ProcessedAt);
