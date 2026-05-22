using PCRC.Model.Clients;

namespace PCRC.ServicesInterface.Clients.Dtos;

public sealed record ClientDto(
    long Id,
    Guid ExternalId,
    string Name,
    ClientStatus Status,
    string? BillingEmail,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int DocumentCount,
    string ProcessingStatus);
