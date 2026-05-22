namespace PCRC.ServicesInterface.Uploads.Dtos;

public sealed record PaymentRecordUploadRequest(
    Guid ClientExternalId,
    IReadOnlyList<UploadFile> Files);
