using PCRC.Model.Cosmos;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IAnalyzerResultRepository
{
    /// 1-RU point read by the hierarchical partition key <c>(clientExternalId, md5)</c>. Returns null
    /// if the document is not present (used by the worker dedup check).
    Task<AnalyzerResult?> GetAsync(Guid clientExternalId, string md5, CancellationToken cancellationToken = default);

    /// Same shape as <see cref="GetAsync"/> but skips deserialization — used when only existence
    /// matters (Contract worker's pre-CU dedup check).
    Task<bool> ExistsAsync(Guid clientExternalId, string md5, CancellationToken cancellationToken = default);

    /// Idempotent create — duplicate writes for the same <c>(clientId, md5)</c> raise
    /// <see cref="System.Net.HttpStatusCode.Conflict"/>, which the caller treats as a benign no-op.
    Task<AnalyzerResult> CreateAsync(AnalyzerResult result, CancellationToken cancellationToken = default);

    /// Use only when re-running with a newer analyzer; the standard worker path uses
    /// <see cref="CreateAsync"/> so a duplicate write is detectable.
    Task<AnalyzerResult> UpsertAsync(AnalyzerResult result, CancellationToken cancellationToken = default);

    /// Removes every Contract result for one client in a single operation. Used by the archive
    /// workflow — relies on partial hierarchical PK delete.
    Task DeleteAllForClientAsync(Guid clientExternalId, CancellationToken cancellationToken = default);
}
