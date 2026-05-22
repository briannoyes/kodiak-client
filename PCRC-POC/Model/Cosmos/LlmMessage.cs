namespace PCRC.Model.Cosmos;

/// One message in an LLM conversation turn's input history. <c>tool</c>-role messages carry the
/// result of a previous turn's tool calls in <see cref="Content"/>.
public class LlmMessage
{
    /// One of <c>system</c>, <c>user</c>, <c>assistant</c>, <c>tool</c>.
    public string Role { get; set; } = default!;

    public string? Content { get; set; }
}
