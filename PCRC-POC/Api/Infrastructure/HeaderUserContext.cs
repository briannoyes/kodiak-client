using PCRC.DataInterface;

namespace PCRC.Api.Infrastructure;

/// POC <see cref="IUserContext"/>. The full Entra-claims-based resolver is follow-up work; until
/// then this reads <c>X-User-Id</c> (internal BIGINT) and <c>X-Entra-Object-Id</c> off the request,
/// so the audit attribution rule (every CreatedAt has a paired ByUserId) is satisfied from day one.
public sealed class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public HeaderUserContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public long? UserId
    {
        get
        {
            var raw = _accessor.HttpContext?.Request.Headers["X-User-Id"].ToString();
            return long.TryParse(raw, out var parsed) ? parsed : null;
        }
    }

    public string? EntraObjectId
        => _accessor.HttpContext?.Request.Headers["X-Entra-Object-Id"].ToString() is { Length: > 0 } v
            ? v
            : null;
}
