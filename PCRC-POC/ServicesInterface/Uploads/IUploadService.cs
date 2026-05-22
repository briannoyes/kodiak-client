using PCRC.ServicesInterface.Uploads.Dtos;

namespace PCRC.ServicesInterface.Uploads;

/// Orchestrates the upload flows from docs/diagrams/src/Kodiak*UploadSequence.puml. Controllers
/// stay thin and delegate every multi-repository step here.
public interface IUploadService
{
    /// Phase 1 of the multi-select contract upload (KodiakMultiSelectContractUploadSequence).
    /// Persists the Upload + per-file Pending Documents, mints a single-blob write-only SAS PUT URL
    /// per file, and returns them so the browser can upload bytes directly to Blob storage with
    /// per-file progress events.
    Task<DirectContractUploadInitiated> BeginDirectContractUploadAsync(
        DirectContractUploadRequest request,
        CancellationToken cancellationToken);

    /// Phase 3 of the multi-select contract upload. The client calls this after PUTting the files
    /// to their SAS URLs; the service promotes those Documents from Pending to Processing and
    /// enqueues each one onto the Document worker queue. Returns null when the Upload isn't found.
    Task<UploadAccepted?> FinalizeDirectContractUploadAsync(
        Guid uploadExternalId,
        DirectContractUploadFinalizeRequest request,
        CancellationToken cancellationToken);

    /// Bulk contract ingest (KodiakBulkContractUploadSequence). Validates the source container SAS,
    /// inserts a Pending Upload row with TotalCount=null, and enqueues a single bulk-ingest message
    /// for the worker to enumerate.
    Task<UploadAccepted> CreateBulkContractUploadAsync(
        BulkContractUploadRequest request,
        CancellationToken cancellationToken);

    /// Payment record upload (KodiakMultiSelectPaymentRecordUploadSequence). For each file: streams
    /// into Blob, reads the header row inline, computes the SHA256 fingerprint, looks for a matching
    /// PaymentMappingTemplate. Matched files land Processing and are enqueued; unmatched files land
    /// AwaitingMapping for the UI mapping step.
    Task<PaymentRecordUploadAccepted> CreatePaymentRecordUploadAsync(
        PaymentRecordUploadRequest request,
        CancellationToken cancellationToken);

    /// Progress polling for any upload source type. Returns null when the upload isn't found.
    Task<UploadProgress?> GetUploadProgressAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken);

    /// Header-group listing for the payment-record mapping UI. Documents grouped by their
    /// HeaderFingerprint, filtered to AwaitingMapping. Returns null when the upload doesn't exist.
    Task<IReadOnlyList<HeaderGroup>?> GetAwaitingMappingHeaderGroupsAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken);

    /// Upserts a PaymentMappingTemplate keyed by (ClientId, HeaderFingerprint), promotes every
    /// AwaitingMapping document in this upload + fingerprint to Processing, and enqueues each one.
    /// Returns false when the upload or the fingerprint group isn't found.
    Task<bool> ApproveHeaderMappingAsync(
        Guid uploadExternalId,
        string headerFingerprint,
        HeaderMappingApproval approval,
        CancellationToken cancellationToken);

    /// Hard-deletes the upload and everything tied to it: documents (and best-effort blob deletes),
    /// payments linked to those documents. Single DbContext transaction. Returns false when the
    /// upload is not found. Cosmos AnalyzerResults are not touched.
    Task<bool> DeleteAsync(Guid uploadExternalId, CancellationToken cancellationToken);

    /// One pass of the orphan sweep for the two-phase direct flow: any Direct upload still Pending
    /// with CreatedAt &lt; <paramref name="cutoff"/> and whose Documents are all still Pending is
    /// marked Failed, its Documents flipped to Failed, and any blobs the client managed to PUT are
    /// deleted. Uploads that have at least one Document past Pending are left alone (partial
    /// finalize). Returns aggregate counts for logging.
    Task<DirectUploadOrphanSweepResult> SweepDirectUploadOrphansAsync(
        DateTime cutoff,
        CancellationToken cancellationToken);
}
