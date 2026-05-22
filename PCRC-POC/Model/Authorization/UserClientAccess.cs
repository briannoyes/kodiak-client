using PCRC.Model.Interfaces;

namespace PCRC.Model.Authorization;

public class UserClientAccess : IHaveId, IHaveExternalId
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public long UserId { get; set; }
    public long ClientId { get; set; }
    public UserClientAccessStatus Status { get; set; }

    public DateTime GrantedAt { get; set; }
    public long? GrantedByUserId { get; set; }

    public DateTime? RevokedAt { get; set; }
    public long? RevokedByUserId { get; set; }
}
