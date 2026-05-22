using Microsoft.Extensions.Logging;
using PCRC.DataInterface;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Documents;
using PCRC.Model.Payments;
using PCRC.Model.Uploads;
using PCRC.ServicesInterface.Messaging;
using PCRC.ServicesInterface.Storage;
using PCRC.ServicesInterface.Uploads;
using PCRC.ServicesInterface.Uploads.Dtos;

namespace PCRC.Services.Uploads;

public sealed class UploadService : IUploadService
{
    private readonly IClientRepository _clients;
    private readonly IUploadRepository _uploads;
    private readonly IDocumentRepository _documents;
    private readonly IPaymentRepository _payments;
    private readonly IPaymentMappingTemplateRepository _mappingTemplates;
    private readonly IBlobStorageClient _blob;
    private readonly IUploadQueueClient _queue;
    private readonly ISourceSasProbeClient _sasProbe;
    private readonly IExcelHeaderReader _excelHeaderReader;
    private readonly IUserContext _userContext;
    private readonly ILogger<UploadService> _logger;

    public UploadService(
        IClientRepository clients,
        IUploadRepository uploads,
        IDocumentRepository documents,
        IPaymentRepository payments,
        IPaymentMappingTemplateRepository mappingTemplates,
        IBlobStorageClient blob,
        IUploadQueueClient queue,
        ISourceSasProbeClient sasProbe,
        IExcelHeaderReader excelHeaderReader,
        IUserContext userContext,
        ILogger<UploadService> logger)
    {
        _clients = clients;
        _uploads = uploads;
        _documents = documents;
        _payments = payments;
        _mappingTemplates = mappingTemplates;
        _blob = blob;
        _queue = queue;
        _sasProbe = sasProbe;
        _excelHeaderReader = excelHeaderReader;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<DirectContractUploadInitiated> BeginDirectContractUploadAsync(
        DirectContractUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(request));

        var (client, userId) = await ResolveClientAndCallerAsync(request.ClientExternalId);

        var upload = await CreateUploadAsync(
            client.Id,
            userId,
            UploadSourceType.Direct,
            totalCount: request.Files.Count);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var slots = new List<DirectContractUploadSlot>(request.Files.Count);

        foreach (var file in request.Files)
        {
            var slot = await _blob.CreateUploadSlotAsync(
                client.ExternalId,
                upload.ExternalId,
                file.FileName,
                file.ContentType,
                expiresAt,
                cancellationToken);

            var document = new Document
            {
                ExternalId = Guid.NewGuid(),
                DocumentType = DocumentType.Contract,
                UploadId = upload.Id,
                ClientId = client.Id,
                ClientExternalId = client.ExternalId,
                UploadedByUserId = userId,
                BlobPath = slot.BlobPath,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.SizeBytes,
                Status = DocumentStatus.Pending,
                UploadedAt = DateTime.UtcNow,
            };
            _documents.Add(document);

            slots.Add(new DirectContractUploadSlot(
                document.ExternalId,
                file.FileName,
                slot.BlobPath,
                slot.SasPutUri,
                expiresAt));
        }
        await _documents.SaveChangesAsync();

        return new DirectContractUploadInitiated(upload.ExternalId, slots);
    }

    public async Task<UploadAccepted?> FinalizeDirectContractUploadAsync(
        Guid uploadExternalId,
        DirectContractUploadFinalizeRequest request,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return null;

        if (upload.SourceType != UploadSourceType.Direct)
            throw new InvalidOperationException(
                $"Upload {uploadExternalId} is not a direct contract upload (SourceType={upload.SourceType}).");

        var documentsByExternalId = (await _documents.GetByUploadAsync(upload.Id))
            .ToDictionary(d => d.ExternalId);

        var toPromote = new List<Document>(request.DocumentExternalIds.Count);
        foreach (var documentExternalId in request.DocumentExternalIds)
        {
            if (!documentsByExternalId.TryGetValue(documentExternalId, out var document))
                throw new KeyNotFoundException(
                    $"Document {documentExternalId} is not part of upload {uploadExternalId}.");

            if (document.Status != DocumentStatus.Pending) continue;
            document.Status = DocumentStatus.Processing;
            _documents.SetModified(document);
            toPromote.Add(document);
        }
        await _documents.SaveChangesAsync();

        foreach (var document in toPromote)
        {
            await _queue.EnqueueAsync(
                UploadQueueNames.ContractDocuments,
                new DocumentQueueMessage(document.Id),
                cancellationToken);
        }

        _logger.LogInformation(
            "Finalized direct upload {UploadId}; promoted {PromotedCount} of {RequestedCount} Documents to Processing.",
            upload.ExternalId, toPromote.Count, request.DocumentExternalIds.Count);

        return new UploadAccepted(upload.ExternalId);
    }

    public async Task<UploadAccepted> CreateBulkContractUploadAsync(
        BulkContractUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourceSasUrl))
            throw new ArgumentException("SourceSasUrl is required.", nameof(request));

        var (client, userId) = await ResolveClientAndCallerAsync(request.ClientExternalId);

        if (!await _sasProbe.CanReadContainerAsync(request.SourceSasUrl, cancellationToken))
            throw new InvalidOperationException("Source container SAS URL is not reachable or lacks list/read permission.");

        var upload = await CreateUploadAsync(
            client.Id,
            userId,
            UploadSourceType.Bulk,
            totalCount: null);

        await _queue.EnqueueAsync(
            UploadQueueNames.BulkIngest,
            new BulkIngestQueueMessage(upload.Id, request.SourceSasUrl, request.PathPrefix, request.Pattern),
            cancellationToken);

        return new UploadAccepted(upload.ExternalId);
    }

    public async Task<PaymentRecordUploadAccepted> CreatePaymentRecordUploadAsync(
        PaymentRecordUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(request));

        var (client, userId) = await ResolveClientAndCallerAsync(request.ClientExternalId);

        var upload = await CreateUploadAsync(
            client.Id,
            userId,
            UploadSourceType.Direct,
            totalCount: request.Files.Count);

        var results = new List<PaymentRecordFileResult>(request.Files.Count);

        foreach (var file in request.Files)
        {
            var (document, requiresMapping) = await StorePaymentRecordFileAsync(
                client.Id,
                client.ExternalId,
                upload,
                userId,
                file,
                cancellationToken);

            results.Add(new PaymentRecordFileResult(
                file.FileName,
                document.ExternalId,
                document.Status,
                requiresMapping));
        }

        return new PaymentRecordUploadAccepted(upload.ExternalId, results);
    }

    public async Task<UploadProgress?> GetUploadProgressAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return null;

        var awaitingMappingCount = 0;
        if (upload.SourceType == UploadSourceType.Direct)
        {
            awaitingMappingCount = (await _documents.GetAwaitingMappingByUploadAsync(upload.Id)).Count;
        }

        return new UploadProgress(
            upload.ExternalId,
            upload.SourceType,
            upload.Status,
            upload.TotalCount,
            upload.ProcessedCount,
            upload.DedupedCount,
            upload.FailedCount,
            awaitingMappingCount,
            upload.CreatedAt,
            upload.CompletedAt);
    }

    public async Task<IReadOnlyList<HeaderGroup>?> GetAwaitingMappingHeaderGroupsAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return null;

        var awaiting = await _documents.GetAwaitingMappingByUploadAsync(upload.Id);

        return awaiting
            .Where(d => !string.IsNullOrEmpty(d.HeaderFingerprint))
            .GroupBy(d => d.HeaderFingerprint!)
            .Select(g => new HeaderGroup(
                g.Key,
                ParseHeaders(g.First().Headers),
                g.Select(d => d.ExternalId).ToList()))
            .ToList();
    }

    public async Task<bool> ApproveHeaderMappingAsync(
        Guid uploadExternalId,
        string headerFingerprint,
        HeaderMappingApproval approval,
        CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return false;

        var awaiting = (await _documents.GetAwaitingMappingByUploadAsync(upload.Id))
            .Where(d => string.Equals(d.HeaderFingerprint, headerFingerprint, StringComparison.Ordinal))
            .ToList();
        if (awaiting.Count == 0) return false;

        var mappingJson = System.Text.Json.JsonSerializer.Serialize(approval.Mapping);

        var template = await _mappingTemplates.GetByClientAndFingerprintAsync(upload.ClientId, headerFingerprint);
        if (template is null)
        {
            template = new PaymentMappingTemplate
            {
                ClientId = upload.ClientId,
                HeaderFingerprint = headerFingerprint,
                Mapping = mappingJson,
            };
            _mappingTemplates.Add(template);
        }
        else
        {
            template.Mapping = mappingJson;
            _mappingTemplates.SetModified(template);
        }
        await _mappingTemplates.SaveChangesAsync();

        foreach (var doc in awaiting)
        {
            doc.MappingTemplateId = template.Id;
            doc.Status = DocumentStatus.Processing;
            _documents.SetModified(doc);
        }
        await _documents.SaveChangesAsync();

        foreach (var doc in awaiting)
        {
            await _queue.EnqueueAsync(
                UploadQueueNames.PaymentRecordParse,
                new PaymentRecordParseQueueMessage(doc.Id),
                cancellationToken);
        }

        _logger.LogInformation(
            "Approved mapping for upload {UploadId} fingerprint {Fingerprint}; promoted {DocCount} documents to Processing.",
            upload.ExternalId, headerFingerprint, awaiting.Count);

        return true;
    }

    public async Task<DirectUploadOrphanSweepResult> SweepDirectUploadOrphansAsync(
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        var candidates = (await _uploads.GetByStatusAsync(UploadStatus.Pending))
            .Where(u => u.SourceType == UploadSourceType.Direct && u.CreatedAt < cutoff)
            .ToList();

        var uploadsSwept = 0;
        var documentsFailed = 0;
        var blobsDeleted = 0;
        var completedAt = DateTime.UtcNow;

        foreach (var upload in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var documents = await _documents.GetByUploadAsync(upload.Id);

            // Partial-finalize guard: skip uploads where at least one Document has progressed past
            // Pending — those still have meaningful state.
            if (documents.Any(d => d.Status != DocumentStatus.Pending)) continue;

            foreach (var document in documents)
            {
                if (!string.IsNullOrEmpty(document.BlobPath)
                    && await _blob.DeleteAsync(document.BlobPath, cancellationToken))
                {
                    blobsDeleted++;
                }

                document.Status = DocumentStatus.Failed;
                document.ErrorMessage = "Direct upload was never finalized; swept as orphan.";
                _documents.SetModified(document);
                documentsFailed++;
            }

            upload.Status = UploadStatus.Failed;
            upload.CompletedAt = completedAt;
            _uploads.SetModified(upload);
            uploadsSwept++;
        }

        if (uploadsSwept > 0)
        {
            await _documents.SaveChangesAsync();
            await _uploads.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Direct-upload orphan sweep: cutoff={Cutoff:o}, uploads={UploadsSwept}, documents={DocumentsFailed}, blobs={BlobsDeleted}.",
            cutoff, uploadsSwept, documentsFailed, blobsDeleted);

        return new DirectUploadOrphanSweepResult(uploadsSwept, documentsFailed, blobsDeleted);
    }

    public async Task<bool> DeleteAsync(Guid uploadExternalId, CancellationToken cancellationToken)
    {
        var upload = await _uploads.GetByExternalIdAsync(uploadExternalId);
        if (upload is null) return false;

        var documents = await _documents.GetByUploadAsync(upload.Id);
        foreach (var document in documents)
        {
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
                        "Failed to delete blob {BlobPath} during upload {UploadExternalId} deletion.",
                        document.BlobPath, uploadExternalId);
                }
            }

            _documents.Remove(document);
        }

        _uploads.Remove(upload);
        await _uploads.SaveChangesAsync();
        return true;
    }

    private async Task<(Model.Clients.Client Client, long UserId)> ResolveClientAndCallerAsync(Guid clientExternalId)
    {
        var client = await _clients.GetByExternalIdAsync(clientExternalId)
            ?? throw new KeyNotFoundException($"Client {clientExternalId} was not found.");

        var userId = _userContext.UserId
            ?? throw new InvalidOperationException("User context is required to initiate an upload but no UserId was resolved.");

        return (client, userId);
    }

    private async Task<Upload> CreateUploadAsync(
        long clientId,
        long userId,
        UploadSourceType sourceType,
        int? totalCount)
    {
        var upload = new Upload
        {
            ExternalId = Guid.NewGuid(),
            ClientId = clientId,
            InitiatedByUserId = userId,
            SourceType = sourceType,
            Status = UploadStatus.Pending,
            TotalCount = totalCount,
            CreatedAt = DateTime.UtcNow,
        };
        _uploads.Add(upload);
        await _uploads.SaveChangesAsync();
        return upload;
    }

    private async Task<(Document Document, bool RequiresMapping)> StorePaymentRecordFileAsync(
        long clientId,
        Guid clientExternalId,
        Upload upload,
        long userId,
        UploadFile file,
        CancellationToken cancellationToken)
    {
        string blobPath;
        IReadOnlyList<string> headers;
        await using (var stream = file.OpenReadStream())
        {
            blobPath = await _blob.WriteAsync(
                clientExternalId,
                upload.ExternalId,
                file.FileName,
                file.ContentType,
                stream,
                cancellationToken);
        }

        await using (var readBackStream = file.OpenReadStream())
        {
            headers = await _excelHeaderReader.ReadFirstRowAsync(readBackStream, cancellationToken);
        }

        var fingerprint = HeaderFingerprintHasher.Compute(headers);
        var template = await _mappingTemplates.GetByClientAndFingerprintAsync(clientId, fingerprint);
        var requiresMapping = template is null;

        var document = new Document
        {
            ExternalId = Guid.NewGuid(),
            DocumentType = DocumentType.PaymentRecord,
            UploadId = upload.Id,
            ClientId = clientId,
            ClientExternalId = clientExternalId,
            UploadedByUserId = userId,
            BlobPath = blobPath,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.SizeBytes,
            Headers = SerializeHeaders(headers),
            HeaderFingerprint = fingerprint,
            MappingTemplateId = template?.Id,
            Status = requiresMapping ? DocumentStatus.AwaitingMapping : DocumentStatus.Processing,
            UploadedAt = DateTime.UtcNow,
        };

        _documents.Add(document);
        await _documents.SaveChangesAsync();

        if (!requiresMapping)
        {
            await _queue.EnqueueAsync(
                UploadQueueNames.PaymentRecordParse,
                new PaymentRecordParseQueueMessage(document.Id),
                cancellationToken);
        }

        return (document, requiresMapping);
    }

    private static string SerializeHeaders(IReadOnlyList<string> headers)
        => System.Text.Json.JsonSerializer.Serialize(headers);

    private static IReadOnlyList<string> ParseHeaders(string? headersJson)
    {
        if (string.IsNullOrEmpty(headersJson)) return Array.Empty<string>();
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(headersJson) ?? new List<string>();
    }
}
