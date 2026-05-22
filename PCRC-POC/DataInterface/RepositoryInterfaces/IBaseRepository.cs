using PCRC.Model.Interfaces;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IBaseRepository<TEntity>
    where TEntity : class, IHaveId
{
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(long id);
    void Add(TEntity t);
    void Remove(TEntity t);
    void SetModified(TEntity t);
    Task AddRangeAsync(IEnumerable<TEntity> t);
    void Detach(TEntity t);
    void ClearChangeTracker();
    Task<int> SaveChangesAsync();
}
