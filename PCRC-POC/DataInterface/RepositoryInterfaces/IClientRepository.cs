using PCRC.Model.Clients;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IClientRepository : IExternallyIdentifiedRepository<Client>
{
    Task<List<Client>> GetByStatusAsync(ClientStatus status);
    Task<List<Client>> GetActiveForUserAsync(long userId);
}
