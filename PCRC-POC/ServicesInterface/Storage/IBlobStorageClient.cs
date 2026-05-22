namespace PCRC.ServicesInterface.Storage;

/// Port for writing uploaded files into Azure Blob Storage. Implementations are expected to stream
/// the source <c>content</c> straight to the destination without buffering the whole payload.
public interface IBlobStorageClient
{
    /// PUTs <paramref name="content"/> to <c>{clientExternalId}/{uploadExternalId}/{fileName}</c>
    /// and returns the canonical BlobPath that should land in Documents.BlobPath.
    Task<string> WriteAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        string fileName,
        string? contentType,
        Stream content,
        CancellationToken cancellationToken);

    /// Mints a single-blob, write-only SAS PUT URL that the browser uses to upload the file directly
    /// to Blob storage (see KodiakMultiSelectContractUploadSequence.puml, Phase 1). Returns the
    /// canonical BlobPath (<c>{clientExternalId}/{uploadExternalId}/{fileName}</c>) that should land
    /// in Documents.BlobPath, paired with the SAS URI the client should PUT to.
    Task<BlobUploadSlot> CreateUploadSlotAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        string fileName,
        string? contentType,
        DateTimeOffset expiresOn,
        CancellationToken cancellationToken);

    /// Best-effort blob delete; returns true when the blob existed and was removed, false otherwise.
    /// Used by the direct-upload orphan sweep to reclaim space when a client called BeginDirect but
    /// never reached Finalize.
    Task<bool> DeleteAsync(string blobPath, CancellationToken cancellationToken);
}

/// A single-blob write SAS issued for one file in a two-phase upload.
public sealed record BlobUploadSlot(string BlobPath, Uri SasPutUri);
