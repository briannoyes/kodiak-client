using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCRC.ServicesInterface.Configuration;
using PCRC.ServicesInterface.Storage;

namespace PCRC.Services.Azure;

public sealed class AzureBlobStorageClient : IBlobStorageClient
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<AzureBlobStorageClient> _logger;
    private int _containerEnsured;

    public AzureBlobStorageClient(
        BlobServiceClient serviceClient,
        IOptions<StorageOptions> options,
        ILogger<AzureBlobStorageClient> logger)
    {
        _container = serviceClient.GetBlobContainerClient(options.Value.BlobContainerName);
        _logger = logger;
    }

    public async Task<string> WriteAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        string fileName,
        string? contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        await EnsureContainerAsync(cancellationToken);

        var blobName = $"{clientExternalId}/{uploadExternalId}/{fileName}";
        var blob = _container.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions();
        if (!string.IsNullOrEmpty(contentType))
            uploadOptions.HttpHeaders = new BlobHttpHeaders { ContentType = contentType };

        await blob.UploadAsync(content, uploadOptions, cancellationToken);
        _logger.LogInformation("Wrote blob {BlobName} to container {Container}.", blobName, _container.Name);
        return blobName;
    }

    public async Task<BlobUploadSlot> CreateUploadSlotAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        string fileName,
        string? contentType,
        DateTimeOffset expiresOn,
        CancellationToken cancellationToken)
    {
        await EnsureContainerAsync(cancellationToken);

        var blobName = $"{clientExternalId}/{uploadExternalId}/{fileName}";
        var blob = _container.GetBlobClient(blobName);

        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException(
                "BlobClient cannot generate a SAS URI — the BlobServiceClient must be constructed with a shared-key " +
                "credential (connection string) or a user-delegation key.");

        var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Create | BlobSasPermissions.Write, expiresOn)
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
        };
        if (!string.IsNullOrEmpty(contentType))
            sasBuilder.ContentType = contentType;

        var sasUri = blob.GenerateSasUri(sasBuilder);
        _logger.LogInformation(
            "Issued SAS PUT slot for blob {BlobName} (expires {ExpiresOn:o}).", blobName, expiresOn);
        return new BlobUploadSlot(blobName, sasUri);
    }

    public async Task<bool> DeleteAsync(string blobPath, CancellationToken cancellationToken)
    {
        var response = await _container.GetBlobClient(blobPath).DeleteIfExistsAsync(
            cancellationToken: cancellationToken);
        if (response.Value)
            _logger.LogInformation("Deleted orphaned blob {BlobName}.", blobPath);
        return response.Value;
    }

    /// First call performs the idempotent CreateIfNotExists; subsequent calls skip the round-trip.
    /// In dev (Azurite) the container won't exist on a fresh start; in cloud the IaC has provisioned
    /// it but CreateIfNotExists is a no-op, so this stays safe to leave on.
    private async Task EnsureContainerAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _containerEnsured, 1, 0) == 0)
        {
            await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
