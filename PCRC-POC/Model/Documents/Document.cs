using PCRC.Model.Interfaces;

namespace PCRC.Model.Documents;

public class Document : IHaveId, IHaveExternalId
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public DocumentType DocumentType { get; set; }
    public long? UploadId { get; set; }

    public long ClientId { get; set; }
    public Guid ClientExternalId { get; set; }
    public long UploadedByUserId { get; set; }

    public string? BlobPath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? Md5Hash { get; set; }

    public string? Headers { get; set; }
    public string? HeaderFingerprint { get; set; }
    public long? MappingTemplateId { get; set; }

    public DocumentStatus Status { get; set; }
    public string? ResultRef { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
