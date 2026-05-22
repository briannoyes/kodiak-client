using PCRC.Model.Uploads;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IUploadRepository : IExternallyIdentifiedRepository<Upload>
{
    Task<List<Upload>> GetByClientAsync(long clientId);
    Task<List<Upload>> GetByInitiatingUserAsync(long userId);
    Task<List<Upload>> GetByStatusAsync(UploadStatus status);

    /// Atomic counter increment for the worker, mirroring the SQL in DataModelSql.md. Returns the
    /// post-update Status so callers know if the upload reached a terminal state.
    Task<UploadStatus?> IncrementProcessedCountAsync(long uploadId);
    Task<UploadStatus?> IncrementDedupedCountAsync(long uploadId);
    Task<UploadStatus?> IncrementFailedCountAsync(long uploadId);
}
