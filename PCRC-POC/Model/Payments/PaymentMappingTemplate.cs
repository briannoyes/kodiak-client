using PCRC.Model.Interfaces;

namespace PCRC.Model.Payments;

public class PaymentMappingTemplate : IHaveId, IHaveExternalId, ICreatable, IModifiable
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public long ClientId { get; set; }
    public string HeaderFingerprint { get; set; } = default!;
    public string Mapping { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedByUserId { get; set; }
}
