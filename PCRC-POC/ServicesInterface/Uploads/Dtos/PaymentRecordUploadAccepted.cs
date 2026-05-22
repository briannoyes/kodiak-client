using PCRC.Model.Documents;

namespace PCRC.ServicesInterface.Uploads.Dtos;

public sealed record PaymentRecordUploadAccepted(
    Guid UploadExternalId,
    IReadOnlyList<PaymentRecordFileResult> Files);

public sealed record PaymentRecordFileResult(
    string OriginalFileName,
    Guid DocumentExternalId,
    DocumentStatus Status,
    bool RequiresMapping);
