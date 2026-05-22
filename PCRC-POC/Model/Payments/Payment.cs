using PCRC.Model.Interfaces;

namespace PCRC.Model.Payments;

public class Payment : IHaveId, IHaveExternalId
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public long DocumentId { get; set; }
    public long ClientId { get; set; }

    public string? VendorID { get; set; }
    public string? VendorName { get; set; }
    public string? Company { get; set; }

    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? CheckDate { get; set; }
    public string? CheckNumber { get; set; }

    public decimal? InvoiceAmount { get; set; }
    public decimal? CheckAmount { get; set; }

    public DateOnly? VoidDate { get; set; }
    public string? CheckStatus { get; set; }
    public bool PhysicianVendor { get; set; }

    public DateTime CreatedAt { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
