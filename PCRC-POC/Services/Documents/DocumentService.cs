using Microsoft.Extensions.Logging;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Documents;
using PCRC.ServicesInterface.Documents;
using PCRC.ServicesInterface.Documents.Dtos;
using PCRC.ServicesInterface.Storage;

namespace PCRC.Services.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly IUploadRepository _uploads;
    private readonly IDocumentRepository _documents;
    private readonly IPaymentRepository _payments;
    private readonly IBlobStorageClient _blob;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IUploadRepository uploads,
        IDocumentRepository documents,
        IPaymentRepository payments,
        IBlobStorageClient blob,
        ILogger<DocumentService> logger)
    {
        _uploads = uploads;
        _documents = documents;
        _payments = payments;
        _blob = blob;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentDto>?> ListByUploadAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return null;
        var documents = await _documents.GetByUploadAsync(upload.Id);
        return documents.Select(ToDto).ToList();
    }

    public async Task<bool> DeleteAsync(
        Guid uploadExternalId,
        Guid documentExternalId,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return false;

        var document = await _documents.GetByExternalIdAsync(documentExternalId);
        if (document is null || document.UploadId != upload.Id) return false;

        foreach (var payment in await _payments.GetByDocumentAsync(document.Id))
            _payments.Remove(payment);

        if (!string.IsNullOrEmpty(document.BlobPath))
        {
            try
            {
                await _blob.DeleteAsync(document.BlobPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to delete blob {BlobPath} during document {DocumentExternalId} deletion.",
                    document.BlobPath, documentExternalId);
            }
        }

        _documents.Remove(document);
        await _documents.SaveChangesAsync();
        return true;
    }

    private static DocumentDto ToDto(Document d) => new(
        d.Id,
        d.ExternalId,
        d.DocumentType,
        d.UploadId,
        d.ClientId,
        d.ClientExternalId,
        d.OriginalFileName,
        d.ContentType,
        d.SizeBytes,
        d.Status,
        d.ErrorMessage,
        d.UploadedAt,
        d.ProcessedAt);
}
