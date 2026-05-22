using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Interfaces;

namespace PCRC.Data.Repositories;

public class ExternallyIdentifiedRepository<T> : EfCoreBaseRepository<T>, IExternallyIdentifiedRepository<T>
    where T : class, IHaveId, IHaveExternalId
{
    protected ExternallyIdentifiedRepository(PcrcDbContext context) : base(context)
    {
    }

    public virtual async Task<T?> GetByExternalIdAsync(Guid externalId)
        => await _set.SingleOrDefaultAsync(x => x.ExternalId == externalId);
}
