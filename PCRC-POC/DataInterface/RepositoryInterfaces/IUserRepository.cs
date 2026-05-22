using PCRC.Model.Users;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IUserRepository : IExternallyIdentifiedRepository<User>
{
    Task<User?> GetByEntraObjectIdAsync(string entraObjectId);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetByClientIdAsync(long clientId);
}
