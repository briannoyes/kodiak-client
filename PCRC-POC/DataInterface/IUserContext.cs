namespace PCRC.DataInterface;

/// Resolves the current request's caller identity. The internal <c>UserId</c> (BIGINT FK to Users.Id)
/// is what audit columns and the auth predicate reference; <c>EntraObjectId</c> is the upstream Entra
/// claim used to resolve <c>UserId</c> at request entry. Both are nullable for system/worker contexts
/// where no user is on the call stack.
public interface IUserContext
{
    long? UserId { get; }
    string? EntraObjectId { get; }
}
