namespace PCRC.Model.Interfaces;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    long? DeletedByUserId { get; set; }
}