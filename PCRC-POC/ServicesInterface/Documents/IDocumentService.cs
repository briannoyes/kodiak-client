using PCRC.ServicesInterface.Documents.Dtos;

namespace PCRC.ServicesInterface.Documents;

public interface IDocumentService
{
    /// Returns null when the upload is not found; otherwise the documents tied to that upload.
    Task<IReadOnlyList<DocumentDto>?> ListByUploadAsync(
        Guid uploadExternalId,
        CancellationToken cancellationToken);

    /// Hard-deletes the document, its Payments, and best-effort deletes the underlying blob.
    /// Returns false when the upload or document is not found, or when the document does not
    /// belong to the given upload. Cosmos AnalyzerResults are not touched.
    Task<bool> DeleteAsync(
        Guid uploadExternalId,
        Guid documentExternalId,
        CancellationToken cancellationToken);
}
