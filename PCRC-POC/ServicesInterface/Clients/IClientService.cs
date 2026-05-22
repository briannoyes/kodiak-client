using PCRC.ServicesInterface.Clients.Dtos;

namespace PCRC.ServicesInterface.Clients;

public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken);

    Task<ClientDto?> GetByExternalIdAsync(Guid externalId, CancellationToken cancellationToken);

    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken);

    /// Hard-deletes the client row. Returns false when the client is not found.
    Task<bool> DeleteAsync(Guid externalId, CancellationToken cancellationToken);

    /// Returns null when the client is not found; otherwise the uploads scoped to that client.
    Task<IReadOnlyList<ClientUploadDto>?> ListUploadsAsync(
        Guid externalId,
        CancellationToken cancellationToken);
}
