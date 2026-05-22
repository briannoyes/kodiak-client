namespace PCRC.ServicesInterface.Uploads.Dtos;

/// Phase-1 response: the Upload + per-file SAS PUT slots the browser uses to upload bytes directly
/// to Blob storage. After all PUTs succeed, the client calls FinalizeDirectContractUploadAsync with
/// the DocumentExternalIds it managed to upload.
public sealed record DirectContractUploadInitiated(
    Guid UploadExternalId,
    IReadOnlyList<DirectContractUploadSlot> Files);

public sealed record DirectContractUploadSlot(
    Guid DocumentExternalId,
    string FileName,
    string BlobPath,
    Uri SasPutUrl,
    DateTimeOffset ExpiresAt);