using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Authorization;
using PCRC.Model.Users;

namespace PCRC.Data.Repositories;

public class UserRepository : ExternallyIdentifiedRepository<User>, IUserRepository
{
    public UserRepository(PcrcDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEntraObjectIdAsync(string entraObjectId)
        => await _set.SingleOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

    public async Task<User?> GetByEmailAsync(string email)
        => await _set.SingleOrDefaultAsync(u => u.Email == email);

    public async Task<List<User>> GetByClientIdAsync(long clientId)
        => await (
            from u in _set
            join uca in _context.UserClientAccess on u.Id equals uca.UserId
            where uca.ClientId == clientId
                  && uca.Status == UserClientAccessStatus.Active
                  && u.DeletedAt == null
            orderby u.DisplayName, u.Email
            select u
        ).ToListAsync();
}
