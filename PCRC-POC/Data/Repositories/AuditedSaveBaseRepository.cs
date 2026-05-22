using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface;
using PCRC.Model.Interfaces;

namespace PCRC.Data.Repositories;

/// Sets the audit fields (CreatedAt/UpdatedAt + their *ByUserId companions) defined by the
/// ICreatable / IModifiable interfaces whenever an entity is added or marked modified through
/// the repository. Mirrors the SignatureApp pattern but uses the BIGINT user-id model from
/// the PCRC SQL schema instead of a username string.
public class AuditedSaveBaseRepository<T> : ExternallyIdentifiedRepository<T>
    where T : class, IHaveId, IHaveExternalId
{
    private readonly IUserContext _userContext;

    protected AuditedSaveBaseRepository(PcrcDbContext context, IUserContext userContext)
        : base(context)
    {
        _userContext = userContext;
    }

    public override void Add(T t)
    {
        base.Add(t);
        StampAuditFields(t);
    }

    public override void SetModified(T t)
    {
        base.SetModified(t);
        StampAuditFields(t);
    }

    private void StampAuditFields(T t)
    {
        var utcNow = DateTime.UtcNow;
        var userId = _userContext.UserId;
        var state = _context.Entry(t).State;

        if (t is ICreatable creatable && state == EntityState.Added)
        {
            creatable.CreatedAt = utcNow;
            creatable.CreatedByUserId = userId;
        }

        if (t is IModifiable modifiable)
        {
            modifiable.UpdatedAt = utcNow;
            if (state == EntityState.Modified)
            {
                modifiable.UpdatedByUserId = userId;
            }
            else if (state == EntityState.Added && modifiable.UpdatedByUserId is null)
            {
                modifiable.UpdatedByUserId = userId;
            }
        }
    }
}
