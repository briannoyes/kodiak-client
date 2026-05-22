using PCRC.Model.Interfaces;

namespace PCRC.Model.Clients;

public class Client : IHaveId, IHaveExternalId, ICreatable, IModifiable, IArchivable
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public string Name { get; set; } = default!;
    public ClientStatus Status { get; set; }
    public string? BillingEmail { get; set; }

    public DateTime CreatedAt { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedByUserId { get; set; }

    public DateTime? ArchivedAt { get; set; }
    public long? ArchivedByUserId { get; set; }
    public string? ArchiveLocation { get; set; }
}