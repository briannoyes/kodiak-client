using PCRC.Model.Authorization;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IUserClientAccessRepository : IExternallyIdentifiedRepository<UserClientAccess>
{
    Task<UserClientAccess?> GetGrantAsync(long userId, long clientId);
    Task<List<long>> GetActiveClientIdsForUserAsync(long userId);
    Task<List<UserClientAccess>> GetActiveGrantsForClientAsync(long clientId);
    Task<List<UserClientAccess>> GetByClientAsync(long clientId);
    Task RevokeAllForClientAsync(long clientId, long? revokedByUserId);
}
