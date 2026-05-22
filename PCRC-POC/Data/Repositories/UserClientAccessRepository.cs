using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Authorization;

namespace PCRC.Data.Repositories;

public class UserClientAccessRepository : ExternallyIdentifiedRepository<UserClientAccess>, IUserClientAccessRepository
{
    public UserClientAccessRepository(PcrcDbContext context) : base(context)
    {
    }

    public async Task<UserClientAccess?> GetGrantAsync(long userId, long clientId)
        => await _set.SingleOrDefaultAsync(x => x.UserId == userId && x.ClientId == clientId);

    public async Task<List<long>> GetActiveClientIdsForUserAsync(long userId)
        => await _set
            .Where(x => x.UserId == userId && x.Status == UserClientAccessStatus.Active)
            .Select(x => x.ClientId)
            .ToListAsync();

    public async Task<List<UserClientAccess>> GetActiveGrantsForClientAsync(long clientId)
        => await _set
            .Where(x => x.ClientId == clientId && x.Status == UserClientAccessStatus.Active)
            .ToListAsync();

    public async Task<List<UserClientAccess>> GetByClientAsync(long clientId)
        => await _set.Where(x => x.ClientId == clientId).ToListAsync();

    public async Task RevokeAllForClientAsync(long clientId, long? revokedByUserId)
    {
        var utcNow = DateTime.UtcNow;
        await _set
            .Where(x => x.ClientId == clientId && x.Status == UserClientAccessStatus.Active)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(x => x.Status, UserClientAccessStatus.Revoked)
                .SetProperty(x => x.RevokedAt, utcNow)
                .SetProperty(x => x.RevokedByUserId, revokedByUserId));
    }
}
