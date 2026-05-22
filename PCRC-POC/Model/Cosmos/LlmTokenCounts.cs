namespace PCRC.Model.Cosmos;

public class LlmTokenCounts
{
    public int Input { get; set; }
    public int Output { get; set; }

    /// Convenience sum, indexed for cost aggregations.
    public int Total { get; set; }
}
