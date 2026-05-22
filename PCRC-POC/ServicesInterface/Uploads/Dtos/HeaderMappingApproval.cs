namespace PCRC.ServicesInterface.Uploads.Dtos;

/// Mapping is keyed by canonical Payment field name (VendorID, VendorName, Company, InvoiceDate,
/// CheckDate, CheckNumber, InvoiceAmount, CheckAmount, VoidDate, CheckStatus, PhysicianVendor) and
/// the value is the source column name from the Excel header row. Stored verbatim as JSON in
/// PaymentMappingTemplates.Mapping.
public sealed record HeaderMappingApproval(
    IReadOnlyDictionary<string, string> Mapping);
