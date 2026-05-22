namespace PCRC.ServicesInterface.Uploads.Dtos;

/// Phase-1 request body for the two-phase direct contract upload
/// (KodiakMultiSelectContractUploadSequence). The client describes the files it wants to upload;
/// the service responds with a per-file SAS PUT slot the browser uploads to directly.
public sealed record DirectContractUploadRequest(
    Guid ClientExternalId,
    IReadOnlyList<DirectContractUploadFile> Files);

/// File metadata supplied by the client at begin time. No payload — bytes flow direct to Blob.
public sealed record DirectContractUploadFile(
    string FileName,
    string? ContentType,
    long? SizeBytes);