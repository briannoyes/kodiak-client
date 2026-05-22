namespace PCRC.ServicesInterface.Messaging;

public static class UploadQueueNames
{
    /// Document worker queue — receives one message per Contract Document that needs OCR/dedup.
    public const string ContractDocuments = "document-queue";

    /// Bulk ingest queue — one message per Upload, with the source container SAS for the worker
    /// to enumerate and fan out.
    public const string BulkIngest = "bulk-ingest-queue";

    /// PaymentRecord parse queue — receives one message per Document once a mapping is in place.
    public const string PaymentRecordParse = "payment-parse-queue";
}
