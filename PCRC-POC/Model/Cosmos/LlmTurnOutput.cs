using System.Text.Json;

namespace PCRC.Model.Cosmos;

public class LlmTurnOutput
{
    /// Text response from the model. Null when the turn ended in tool calls only.
    public string? Content { get; set; }

    /// Structured tool calls emitted by the model. Vendor-specific JSON.
    public List<JsonElement> ToolCalls { get; set; } = new();

    /// API-reported finish reason: <c>stop</c>, <c>tool_calls</c>, <c>length</c>, <c>content_filter</c>, etc.
    public string? FinishReason { get; set; }
}
