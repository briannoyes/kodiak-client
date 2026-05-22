namespace PCRC.DataInterface.Configuration;

public class CosmosOptions
{
    public const string SectionName = "Cosmos";

    public string EndpointUri { get; set; } = default!;

    /// Account key. When null/empty the data layer will fall back to <c>DefaultAzureCredential</c>
    /// (managed identity in Azure, developer credentials locally).
    public string? AccountKey { get; set; }

    public string DatabaseName { get; set; } = default!;

    public string AnalyzerResultsContainerName { get; set; } = "analyzerResults";
    public string LlmConversationTurnsContainerName { get; set; } = "llmConversationTurns";
}
