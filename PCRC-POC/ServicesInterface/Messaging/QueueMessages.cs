namespace PCRC.ServicesInterface.Messaging;

/// Payload sent to the Document worker for a Contract that's already in Blob.
public sealed record DocumentQueueMessage(long DocumentId);

/// Payload sent to the Document worker for a Contract that lives in a customer-supplied SAS
/// container; the worker performs a server-side copy first.
public sealed record DocumentQueueMessageWithSource(long DocumentId, string SourceBlobUrl);

/// Payload sent to the Bulk Ingest worker; it enumerates and fans out per-document messages.
public sealed record BulkIngestQueueMessage(
    long UploadId,
    string SourceSasUrl,
    string? PathPrefix,
    string? Pattern);

/// Payload sent to the PaymentRecord parse worker once a mapping has been resolved.
public sealed record PaymentRecordParseQueueMessage(long DocumentId);
