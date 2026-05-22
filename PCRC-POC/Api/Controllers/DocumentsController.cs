using Microsoft.AspNetCore.Mvc;
using PCRC.ServicesInterface.Documents;
using PCRC.ServicesInterface.Documents.Dtos;

namespace PCRC.Api.Controllers;

[ApiController]
[Route("api/uploads/{uploadExternalId:guid}/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;

    public DocumentsController(IDocumentService documents)
    {
        _documents = documents;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetDocumentsForUpload(
        Guid uploadExternalId,
        CancellationToken cancellationToken)
    {
        var docs = await _documents.ListByUploadAsync(uploadExternalId, cancellationToken);
        return docs is null ? NotFound() : Ok(docs);
    }

    [HttpDelete("{documentExternalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        Guid uploadExternalId,
        Guid documentExternalId,
        CancellationToken cancellationToken)
        => await _documents.DeleteAsync(uploadExternalId, documentExternalId, cancellationToken)
            ? NoContent()
            : NotFound();
}
