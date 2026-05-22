namespace PCRC.Model.Interfaces;

public interface IModifiable
{
    DateTime UpdatedAt { get; set; }
    long? UpdatedByUserId { get; set; }
}