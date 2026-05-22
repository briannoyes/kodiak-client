using System.Text.Json;

namespace PCRC.Model.Cosmos;

public class LlmTurnInput
{
    public List<LlmMessage> Messages { get; set; } = new();

    /// Tool definitions available on this turn. Vendor-specific JSON; left as <see cref="JsonElement"/>
    /// so we don't pin to OpenAI/Anthropic shapes in this layer.
    public List<JsonElement> Tools { get; set; } = new();
}
