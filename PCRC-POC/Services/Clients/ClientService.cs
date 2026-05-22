using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Clients;
using PCRC.Model.Uploads;
using PCRC.ServicesInterface.Clients;
using PCRC.ServicesInterface.Clients.Dtos;

namespace PCRC.Services.Clients;

public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clients;
    private readonly IUploadRepository _uploads;
    private readonly IDocumentRepository _documents;
    private readonly IPaymentRepository _payments;
    private readonly IPaymentMappingTemplateRepository _mappingTemplates;
    private readonly IUserClientAccessRepository _userClientAccess;

    public ClientService(
        IClientRepository clients,
        IUploadRepository uploads,
        IDocumentRepository documents,
        IPaymentRepository payments,
        IPaymentMappingTemplateRepository mappingTemplates,
        IUserClientAccessRepository userClientAccess)
    {
        _clients = clients;
        _uploads = uploads;
        _documents = documents;
        _payments = payments;
        _mappingTemplates = mappingTemplates;
        _userClientAccess = userClientAccess;
    }

    public async Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken)
    {
        var clients = await _clients.GetAllAsync();
        var dtos = new List<ClientDto>(clients.Count);
        foreach (var client in clients)
        {
            var documentCount = (await _documents.GetByClientAsync(client.Id)).Count;
            dtos.Add(ToDto(client, documentCount));
        }
        return dtos;
    }

    public async Task<ClientDto?> GetByExternalIdAsync(
        Guid externalId,
        CancellationToken cancellationToken)
    {
        var client = await _clients.GetByExternalIdAsync(externalId);
        if (client is null) return null;
        var documentCount = (await _documents.GetByClientAsync(client.Id)).Count;
        return ToDto(client, documentCount);
    }

    public async Task<ClientDto> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Name = request.Name,
            BillingEmail = request.BillingEmail,
            Status = ClientStatus.Active,
        };
        _clients.Add(client);
        await _clients.SaveChangesAsync();
        return ToDto(client, documentCount: 0);
    }

    /// Cascade-deletes everything tied to the client in a single DbContext transaction: Payments,
    /// Documents, Uploads, PaymentMappingTemplates, UserClientAccess grants, then the Client row
    /// itself. Blob storage and Cosmos AnalyzerResults are NOT touched here.
    public async Task<bool> DeleteAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var client = await _clients.GetByExternalIdAsync(externalId);
        if (client is null) return false;

        foreach (var payment in await _payments.GetByClientAsync(client.Id))
            _payments.Remove(payment);

        foreach (var document in await _documents.GetByClientAsync(client.Id))
            _documents.Remove(document);

        foreach (var upload in await _uploads.GetByClientAsync(client.Id))
            _uploads.Remove(upload);

        foreach (var template in await _mappingTemplates.GetByClientAsync(client.Id))
            _mappingTemplates.Remove(template);

        foreach (var grant in await _userClientAccess.GetByClientAsync(client.Id))
            _userClientAccess.Remove(grant);

        _clients.Remove(client);
        await _clients.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<ClientUploadDto>?> ListUploadsAsync(
        Guid externalId,
        CancellationToken cancellationToken)
    {
        var client = await _clients.GetByExternalIdAsync(externalId);
        if (client is null) return null;
        var uploads = await _uploads.GetByClientAsync(client.Id);
        var dtos = new List<ClientUploadDto>(uploads.Count);
        foreach (var upload in uploads)
        {
            var awaitingMappingCount = (await _documents.GetAwaitingMappingByUploadAsync(upload.Id)).Count;
            dtos.Add(ToDto(upload, awaitingMappingCount));
        }
        return dtos;
    }

    private static ClientDto ToDto(Client c, int documentCount) => new(
        c.Id,
        c.ExternalId,
        c.Name,
        c.Status,
        c.BillingEmail,
        c.CreatedAt,
        c.UpdatedAt,
        documentCount,
        c.Status.ToString());

    private static ClientUploadDto ToDto(Upload u, int awaitingMappingCount) => new(
        u.Id,
        u.ExternalId,
        u.ClientId,
        u.SourceType,
        u.Status,
        u.TotalCount,
        u.ProcessedCount,
        u.DedupedCount,
        u.FailedCount,
        awaitingMappingCount,
        u.CreatedAt,
        u.CompletedAt);
}
