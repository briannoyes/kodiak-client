using PCRC.Model.Interfaces;

namespace PCRC.Model.Uploads;

public class Upload : IHaveId, IHaveExternalId
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public long ClientId { get; set; }
    public long InitiatedByUserId { get; set; }

    public UploadSourceType SourceType { get; set; }
    public UploadStatus Status { get; set; }

    public int? TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public int DedupedCount { get; set; }
    public int FailedCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
