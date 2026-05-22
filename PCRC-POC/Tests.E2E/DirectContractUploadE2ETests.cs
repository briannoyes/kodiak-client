using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PCRC.Data;
using PCRC.Model.Documents;
using PCRC.Model.Uploads;
using PCRC.ServicesInterface.Uploads;
using PCRC.ServicesInterface.Uploads.Dtos;
using PCRC.Tests.E2E.Infrastructure;
using Xunit;

namespace PCRC.Tests.E2E;

[Collection(DatabaseCollection.Name)]
public sealed class DirectContractUploadE2ETests
{
    private readonly DatabaseFixture _fixture;

    public DirectContractUploadE2ETests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// Walks the full two-phase direct flow end-to-end:
    ///   1. POST /api/uploads/direct/begin       — server mints N SAS PUT slots
    ///   2. N parallel BlockBlobClient.UploadAsync calls with ProgressHandler
    ///   3. POST /api/uploads/direct/{id}/finalize — server promotes Documents and enqueues them
    /// then asserts per-file progress fired, Documents are Processing, and blobs exist.
    [Fact]
    public async Task BeginDirect_ParallelPutsWithProgress_FinalizePromotesDocuments()
    {
        const int fileCount = 5;
        const int fileSize = 256 * 1024;
        const int chunkSize = 64 * 1024;

        using var http = _fixture.Factory.CreateClient();
        http.DefaultRequestHeaders.Add("X-User-Id", _fixture.SeededUserId.ToString());
        http.DefaultRequestHeaders.Add("X-Entra-Object-Id", _fixture.SeededUserEntraObjectId);

        var sourceFiles = Enumerable.Range(0, fileCount)
            .Select(i => new SourceFile(
                FileName: $"contract-{i}-{Guid.NewGuid():N}.pdf",
                ContentType: "application/pdf",
                Bytes: RandomBytes(fileSize)))
            .ToList();

        // ---- Phase 1: Begin ----
        var beginBody = new DirectContractUploadRequest(
            _fixture.SeededClientExternalId,
            sourceFiles.Select(f => new DirectContractUploadFile(f.FileName, f.ContentType, f.Bytes.Length)).ToList());

        var beginResp = await http.PostAsJsonAsync("/api/uploads/direct/begin", beginBody);
        Assert.Equal(HttpStatusCode.Accepted, beginResp.StatusCode);
        var initiated = await beginResp.Content.ReadFromJsonAsync<DirectContractUploadInitiated>();
        Assert.NotNull(initiated);
        Assert.Equal(fileCount, initiated!.Files.Count);
        Assert.All(initiated.Files, slot =>
        {
            Assert.NotNull(slot.SasPutUrl);
            Assert.False(string.IsNullOrEmpty(slot.BlobPath));
            Assert.True(slot.ExpiresAt > DateTimeOffset.UtcNow);
        });

        // ---- Phase 2: parallel PUTs with progress ----
        var perFileProgress = new ConcurrentDictionary<Guid, List<long>>();

        await Parallel.ForEachAsync(initiated.Files, async (slot, _) =>
        {
            var source = sourceFiles.Single(f => f.FileName == slot.FileName);
            var events = perFileProgress.GetOrAdd(slot.DocumentExternalId, _ => new List<long>());

            var progressHandler = new Progress<long>(bytes =>
            {
                lock (events) events.Add(bytes);
            });

            var blockBlob = new BlockBlobClient(slot.SasPutUrl);
            using var ms = new MemoryStream(source.Bytes, writable: false);

            // Force chunked uploads so we observe multiple progress events per file rather than one
            // single "done" event the SDK fires for small payloads.
            var uploadOptions = new BlobUploadOptions
            {
                ProgressHandler = progressHandler,
                TransferOptions = new Azure.Storage.StorageTransferOptions
                {
                    InitialTransferSize = chunkSize,
                    MaximumTransferSize = chunkSize,
                    MaximumConcurrency = 2,
                },
                HttpHeaders = new BlobHttpHeaders { ContentType = source.ContentType },
            };
            await blockBlob.UploadAsync(ms, uploadOptions, CancellationToken.None);
        });

        Assert.All(initiated.Files, slot =>
        {
            Assert.True(perFileProgress.TryGetValue(slot.DocumentExternalId, out var events),
                $"No progress events captured for {slot.FileName}.");
            Assert.NotEmpty(events!);
            Assert.Equal(fileSize, events!.Max());
            Assert.True(events.SequenceEqual(events.OrderBy(b => b)),
                $"Progress for {slot.FileName} should be monotonically non-decreasing.");
        });

        // ---- Phase 3: Finalize ----
        var finalizeBody = new DirectContractUploadFinalizeRequest(
            initiated.Files.Select(f => f.DocumentExternalId).ToList());
        var finalizeResp = await http.PostAsJsonAsync(
            $"/api/uploads/direct/{initiated.UploadExternalId}/finalize",
            finalizeBody);
        Assert.Equal(HttpStatusCode.Accepted, finalizeResp.StatusCode);

        // ---- Verify ----
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PcrcDbContext>();

        var documentIds = initiated.Files.Select(f => f.DocumentExternalId).ToList();
        var documents = await db.Documents
            .Where(d => documentIds.Contains(d.ExternalId))
            .ToListAsync();

        Assert.Equal(fileCount, documents.Count);
        Assert.All(documents, d =>
        {
            Assert.Equal(DocumentStatus.Processing, d.Status);
            Assert.Equal(DocumentType.Contract, d.DocumentType);
            Assert.False(string.IsNullOrEmpty(d.BlobPath));
        });

        var upload = await db.Uploads.SingleAsync(u => u.ExternalId == initiated.UploadExternalId);
        Assert.Equal(UploadSourceType.Direct, upload.SourceType);
        Assert.Equal(fileCount, upload.TotalCount);

        var blobService = new BlobServiceClient(_fixture.Factory.StorageConnectionString);
        var container = blobService.GetBlobContainerClient(_fixture.Factory.BlobContainerName);
        foreach (var doc in documents)
        {
            var blob = container.GetBlobClient(doc.BlobPath!);
            Assert.True((await blob.ExistsAsync()).Value, $"Blob {doc.BlobPath} should exist.");
        }
    }

    /// Drives the orphan sweep: Begin, PUT one of N files, never Finalize, then directly invoke
    /// IUploadService.SweepDirectUploadOrphansAsync with a future cutoff. The Upload should flip
    /// to Failed and the uploaded blob should be gone.
    [Fact]
    public async Task SweepDirectUploadOrphans_DropsPendingUploadAndDeletesBlobs()
    {
        const int fileCount = 3;
        const int fileSize = 32 * 1024;

        using var http = _fixture.Factory.CreateClient();
        http.DefaultRequestHeaders.Add("X-User-Id", _fixture.SeededUserId.ToString());
        http.DefaultRequestHeaders.Add("X-Entra-Object-Id", _fixture.SeededUserEntraObjectId);

        var sourceFiles = Enumerable.Range(0, fileCount)
            .Select(i => new SourceFile(
                FileName: $"orphan-{i}-{Guid.NewGuid():N}.pdf",
                ContentType: "application/pdf",
                Bytes: RandomBytes(fileSize)))
            .ToList();

        var beginResp = await http.PostAsJsonAsync("/api/uploads/direct/begin",
            new DirectContractUploadRequest(
                _fixture.SeededClientExternalId,
                sourceFiles.Select(f => new DirectContractUploadFile(f.FileName, f.ContentType, f.Bytes.Length)).ToList()));
        beginResp.EnsureSuccessStatusCode();
        var initiated = (await beginResp.Content.ReadFromJsonAsync<DirectContractUploadInitiated>())!;

        // Client crashed after uploading only the first file.
        var uploadedSlot = initiated.Files[0];
        var uploadedSource = sourceFiles.Single(f => f.FileName == uploadedSlot.FileName);
        using (var ms = new MemoryStream(uploadedSource.Bytes))
        {
            await new BlockBlobClient(uploadedSlot.SasPutUrl).UploadAsync(
                ms, new BlobUploadOptions(), CancellationToken.None);
        }

        // Run sweep with a cutoff just past the upload's CreatedAt.
        using var scope = _fixture.Factory.Services.CreateScope();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadService>();
        var result = await uploadService.SweepDirectUploadOrphansAsync(
            cutoff: DateTime.UtcNow.AddMinutes(1),
            CancellationToken.None);

        Assert.Equal(1, result.UploadsSwept);
        Assert.Equal(fileCount, result.DocumentsFailed);
        Assert.Equal(1, result.BlobsDeleted);

        var db = scope.ServiceProvider.GetRequiredService<PcrcDbContext>();
        var upload = await db.Uploads
            .AsNoTracking()
            .SingleAsync(u => u.ExternalId == initiated.UploadExternalId);
        Assert.Equal(UploadStatus.Failed, upload.Status);
        Assert.NotNull(upload.CompletedAt);

        var documents = await db.Documents
            .AsNoTracking()
            .Where(d => d.UploadId == upload.Id)
            .ToListAsync();
        Assert.All(documents, d => Assert.Equal(DocumentStatus.Failed, d.Status));

        var blobService = new BlobServiceClient(_fixture.Factory.StorageConnectionString);
        var container = blobService.GetBlobContainerClient(_fixture.Factory.BlobContainerName);
        Assert.False((await container.GetBlobClient(uploadedSlot.BlobPath).ExistsAsync()).Value,
            "Uploaded blob should have been deleted by the sweep.");
    }

    private static byte[] RandomBytes(int size)
    {
        var buf = new byte[size];
        Random.Shared.NextBytes(buf);
        return buf;
    }

    private sealed record SourceFile(string FileName, string ContentType, byte[] Bytes);
}