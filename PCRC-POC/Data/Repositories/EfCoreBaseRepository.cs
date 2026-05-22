using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Interfaces;

namespace PCRC.Data.Repositories;

public class EfCoreBaseRepository<T> : IBaseRepository<T>
    where T : class, IHaveId
{
    protected readonly PcrcDbContext _context;
    protected readonly DbSet<T> _set;

    protected EfCoreBaseRepository(PcrcDbContext context)
    {
        _context = context;
        _set = _context.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync()
        => await _set.OrderBy(x => x.Id).ToListAsync();

    public virtual async Task<T?> GetByIdAsync(long id)
        => await _set.FindAsync(id);

    public virtual void Add(T t) => _set.Add(t);

    public virtual void Remove(T t) => _set.Remove(t);

    public virtual async Task AddRangeAsync(IEnumerable<T> t)
        => await _set.AddRangeAsync(t);

    public virtual void Detach(T t)
        => _context.Entry(t).State = EntityState.Detached;

    public virtual void ClearChangeTracker()
        => _context.ChangeTracker.Clear();

    public virtual async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public virtual void SetModified(T t)
        => _context.Entry(t).State = EntityState.Modified;
}
