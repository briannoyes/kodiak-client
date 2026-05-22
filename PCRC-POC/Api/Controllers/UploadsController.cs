using Microsoft.AspNetCore.Mvc;
using PCRC.ServicesInterface.Uploads;
using PCRC.ServicesInterface.Uploads.Dtos;

namespace PCRC.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadsController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    /// Phase 1 of the multi-select contract upload — see KodiakMultiSelectContractUploadSequence.puml.
    /// Returns one SAS PUT slot per requested file; the browser uploads bytes directly to Blob.
    [HttpPost("direct/begin")]
    [ProducesResponseType(typeof(DirectContractUploadInitiated), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BeginDirectContractUpload(
        [FromBody] DirectContractUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0) return BadRequest("At least one file is required.");

        var result = await _uploadService.BeginDirectContractUploadAsync(request, cancellationToken);
        return Accepted(result);
    }

    /// Phase 3 of the multi-select contract upload — called after the browser has PUT each file to
    /// its SAS slot. Promotes the listed Documents from Pending to Processing and enqueues each one.
    [HttpPost("direct/{uploadExternalId:guid}/finalize")]
    [ProducesResponseType(typeof(UploadAccepted), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizeDirectContractUpload(
        Guid uploadExternalId,
        [FromBody] DirectContractUploadFinalizeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _uploadService.FinalizeDirectContractUploadAsync(
            uploadExternalId, request, cancellationToken);
        return result is null ? NotFound() : Accepted(result);
    }

    /// Bulk contract ingest via customer-supplied container SAS — see
    /// KodiakBulkContractUploadSequence.puml.
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(UploadAccepted), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreateBulkContractUpload(
        [FromBody] BulkContractUploadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _uploadService.CreateBulkContractUploadAsync(request, cancellationToken);
        return Accepted(result);
    }

    /// Multi-select payment-record upload (Excel + inline header fingerprint) — see
    /// KodiakMultiSelectPaymentRecordUploadSequence.puml.
    [HttpPost("payment-records")]
    [RequestSizeLimit(long.MaxValue)]
    [ProducesResponseType(typeof(PaymentRecordUploadAccepted), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreatePaymentRecordUpload(
        [FromForm] Guid clientExternalId,
        IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        if (files.Count == 0) return BadRequest("At least one file is required.");

        var request = new PaymentRecordUploadRequest(
            clientExternalId,
            files.Select(ToUploadFile).ToList());

        var result = await _uploadService.CreatePaymentRecordUploadAsync(request, cancellationToken);
        return Accepted(result);
    }

    /// Progress polling — see the "Progress polling" section in every upload sequence diagram.
    [HttpGet("{uploadExternalId:guid}")]
    [ProducesResponseType(typeof(UploadProgress), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UploadProgress>> GetUploadProgress(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var progress = await _uploadService.GetUploadProgressAsync(uploadExternalId, cancellationToken);
        return progress is null ? NotFound() : Ok(progress);
    }

    [HttpDelete("{uploadExternalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUpload(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
        => await _uploadService.DeleteAsync(uploadExternalId, cancellationToken)
            ? NoContent()
            : NotFound();

    /// Header groups awaiting mapping for a payment-record upload — see the "UI: header mapping"
    /// section in KodiakMultiSelectPaymentRecordUploadSequence.puml.
    [HttpGet("{uploadExternalId:guid}/header-groups")]
    [ProducesResponseType(typeof(IReadOnlyList<HeaderGroup>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HeaderGroup>>> GetAwaitingMappingHeaderGroups(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var groups = await _uploadService.GetAwaitingMappingHeaderGroupsAsync(uploadExternalId, cancellationToken);
        return groups is null ? NotFound() : Ok(groups);
    }

    /// Approve a header mapping; upserts the PaymentMappingTemplate, promotes matching Documents
    /// to Processing, and enqueues each one for the parse worker.
    [HttpPost("{uploadExternalId:guid}/header-groups/{fingerprint}/mapping")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveHeaderMapping(
        Guid uploadExternalId,
        string fingerprint,
        [FromBody] HeaderMappingApproval approval,
        CancellationToken cancellationToken)
    {
        var ok = await _uploadService.ApproveHeaderMappingAsync(uploadExternalId, fingerprint, approval, cancellationToken);
        return ok ? Accepted() : NotFound();
    }

    private static UploadFile ToUploadFile(IFormFile file) => new(
        file.FileName,
        file.ContentType,
        file.Length,
        file.OpenReadStream);
}
