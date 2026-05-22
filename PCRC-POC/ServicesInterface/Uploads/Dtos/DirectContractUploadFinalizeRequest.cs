namespace PCRC.ServicesInterface.Uploads.Dtos;

/// Phase-3 request: the DocumentExternalIds the client successfully PUT to Blob. The service flips
/// each from Pending to Processing and enqueues it for the Document worker.
public sealed record DirectContractUploadFinalizeRequest(
    IReadOnlyList<Guid> DocumentExternalIds);