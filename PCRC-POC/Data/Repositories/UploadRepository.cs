using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Uploads;

namespace PCRC.Data.Repositories;

public class UploadRepository : ExternallyIdentifiedRepository<Upload>, IUploadRepository
{
    public UploadRepository(PcrcDbContext context) : base(context)
    {
    }

    public async Task<List<Upload>> GetByClientAsync(long clientId)
        => await _set.Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<List<Upload>> GetByInitiatingUserAsync(long userId)
        => await _set.Where(x => x.InitiatedByUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<List<Upload>> GetByStatusAsync(UploadStatus status)
        => await _set.Where(x => x.Status == status)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public Task<UploadStatus?> IncrementProcessedCountAsync(long uploadId)
        => IncrementCounterAsync(uploadId, "ProcessedCount");

    public Task<UploadStatus?> IncrementDedupedCountAsync(long uploadId)
        => IncrementCounterAsync(uploadId, "DedupedCount");

    public Task<UploadStatus?> IncrementFailedCountAsync(long uploadId)
        => IncrementCounterAsync(uploadId, "FailedCount");

    /// Single round-trip atomic counter increment matching the SQL pattern in DataModelSql.md.
    /// Updates the named counter, flips Status to 'Completed' / sets CompletedAt when the sum of
    /// the three counters equals TotalCount, and returns the post-update Status.
    private async Task<UploadStatus?> IncrementCounterAsync(long uploadId, string counterColumn)
    {
        var sql = $@"
UPDATE Uploads
SET {counterColumn} = {counterColumn} + 1,
    Status = CASE
        WHEN ProcessedCount + DedupedCount + FailedCount + 1 = TotalCount
            THEN 'Completed' ELSE 'Processing'
    END,
    CompletedAt = CASE
        WHEN ProcessedCount + DedupedCount + FailedCount + 1 = TotalCount
            THEN SYSUTCDATETIME() ELSE CompletedAt
    END
OUTPUT INSERTED.Status
WHERE Id = @id;";

        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new SqlParameter("@id", uploadId));

        await _context.Database.OpenConnectionAsync();
        try
        {
            var result = await command.ExecuteScalarAsync();
            return result is string statusName && Enum.TryParse<UploadStatus>(statusName, out var status)
                ? status
                : null;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}
