namespace PCRC.Model.Interfaces;

public interface ICreatable
{
    DateTime CreatedAt { get; set; }
    long? CreatedByUserId { get; set; }
}