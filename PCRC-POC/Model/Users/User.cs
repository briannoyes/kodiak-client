using PCRC.Model.Interfaces;

namespace PCRC.Model.Users;

public class User : IHaveId, IHaveExternalId, ISoftDeletable
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public string EntraObjectId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public long? CreatedByUserId { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public long? DeletedByUserId { get; set; }
}