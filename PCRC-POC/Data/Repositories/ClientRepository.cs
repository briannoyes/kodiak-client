using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Authorization;
using PCRC.Model.Clients;

namespace PCRC.Data.Repositories;

public class ClientRepository : AuditedSaveBaseRepository<Client>, IClientRepository
{
    public ClientRepository(PcrcDbContext context, IUserContext userContext)
        : base(context, userContext)
    {
    }

    public async Task<List<Client>> GetByStatusAsync(ClientStatus status)
        => await _set.Where(x => x.Status == status)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<List<Client>> GetActiveForUserAsync(long userId)
        => await (
            from c in _set
            join uca in _context.UserClientAccess on c.Id equals uca.ClientId
            where uca.UserId == userId
                  && uca.Status == UserClientAccessStatus.Active
                  && c.Status != ClientStatus.Archived
            orderby c.Name
            select c
        ).ToListAsync();
}
