namespace PCRC.ServicesInterface.Uploads.Dtos;

public sealed record BulkContractUploadRequest(
    Guid ClientExternalId,
    string SourceSasUrl,
    string? PathPrefix,
    string? Pattern);
