namespace PCRC.ServicesInterface.Uploads.Dtos;

public sealed record HeaderGroup(
    string Fingerprint,
    IReadOnlyList<string> Headers,
    IReadOnlyList<Guid> DocumentExternalIds);
