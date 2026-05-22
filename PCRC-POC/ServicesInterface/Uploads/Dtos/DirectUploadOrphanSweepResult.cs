namespace PCRC.ServicesInterface.Uploads.Dtos;

/// Outcome of one pass of the direct-upload orphan sweep
/// (see KodiakMultiSelectContractUploadSequence.puml — "Orphan sweep" section).
public sealed record DirectUploadOrphanSweepResult(
    int UploadsSwept,
    int DocumentsFailed,
    int BlobsDeleted);