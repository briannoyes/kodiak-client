using PCRC.Model.Documents;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IDocumentRepository : IExternallyIdentifiedRepository<Document>
{
    Task<List<Document>> GetByClientAsync(long clientId);
    Task<List<Document>> GetByClientAndTypeAsync(long clientId, DocumentType documentType);
    Task<List<Document>> GetByUploadAsync(long uploadId);
    Task<List<Document>> GetByUploadedUserAsync(long userId);
    Task<List<Document>> GetAwaitingMappingByUploadAsync(long uploadId);

    /// Used by the Contract worker dedup check: existence of a completed Document with the same
    /// (ClientId, Md5Hash) pair. Cosmos point-read is the authoritative dedup; this is the SQL fallback
    /// used by the PaymentRecord path.
    Task<Document?> FindCompletedByHashAsync(long clientId, string md5Hash);
}
