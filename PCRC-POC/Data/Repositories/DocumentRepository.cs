using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Documents;

namespace PCRC.Data.Repositories;

public class DocumentRepository : ExternallyIdentifiedRepository<Document>, IDocumentRepository
{
    public DocumentRepository(PcrcDbContext context) : base(context)
    {
    }

    public async Task<List<Document>> GetByClientAsync(long clientId)
        => await _set.Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

    public async Task<List<Document>> GetByClientAndTypeAsync(long clientId, DocumentType documentType)
        => await _set.Where(x => x.ClientId == clientId && x.DocumentType == documentType)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

    public async Task<List<Document>> GetByUploadAsync(long uploadId)
        => await _set.Where(x => x.UploadId == uploadId)
            .OrderBy(x => x.UploadedAt)
            .ToListAsync();

    public async Task<List<Document>> GetByUploadedUserAsync(long userId)
        => await _set.Where(x => x.UploadedByUserId == userId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();

    public async Task<List<Document>> GetAwaitingMappingByUploadAsync(long uploadId)
        => await _set.Where(x => x.UploadId == uploadId
                                 && x.Status == DocumentStatus.AwaitingMapping
                                 && x.DocumentType == DocumentType.PaymentRecord)
            .ToListAsync();

    public async Task<Document?> FindCompletedByHashAsync(long clientId, string md5Hash)
        => await _set.FirstOrDefaultAsync(x => x.ClientId == clientId
                                               && x.Md5Hash == md5Hash
                                               && x.Status == DocumentStatus.Completed);
}
