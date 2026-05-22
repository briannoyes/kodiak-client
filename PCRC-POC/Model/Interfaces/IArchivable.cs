namespace PCRC.Model.Interfaces;

public interface IArchivable
{
    DateTime? ArchivedAt { get; set; }
    long? ArchivedByUserId { get; set; }
}