using PCRC.Model.Interfaces;

namespace PCRC.DataInterface.RepositoryInterfaces;

/// Convenience contract for the dominant access pattern in this system: every public-facing row
/// is looked up by its <c>ExternalId</c> (UUID), never by its internal BIGINT <c>Id</c>.
public interface IExternallyIdentifiedRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class, IHaveId, IHaveExternalId
{
    Task<TEntity?> GetByExternalIdAsync(Guid externalId);
}
