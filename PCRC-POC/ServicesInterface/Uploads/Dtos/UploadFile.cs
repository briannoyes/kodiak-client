namespace PCRC.ServicesInterface.Uploads.Dtos;

/// One file part presented to the upload service. <c>OpenReadStream</c> is invoked once per file
/// so the service can stream straight into Blob storage without buffering through memory.
public sealed record UploadFile(
    string FileName,
    string? ContentType,
    long? SizeBytes,
    Func<Stream> OpenReadStream);
