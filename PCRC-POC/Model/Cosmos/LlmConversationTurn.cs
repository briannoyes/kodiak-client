namespace PCRC.Model.Cosmos;

/// One row in the <c>llmConversationTurns</c> container — a single LLM API call (single-shot or one
/// round in a multi-turn agent). Hierarchical partition key is <c>(clientId, conversationId)</c>;
/// <see cref="Id"/> is a per-turn UUID so reads are 1-RU point lookups.
public class LlmConversationTurn
{
    /// Per-turn UUID. Generated client-side at call time.
    public string Id { get; set; } = default!;

    /// SQL <c>Clients.ExternalId</c> as a string UUID. First level of the hierarchical partition key.
    public string ClientId { get; set; } = default!;

    /// Conversation UUID — groups all turns of one logical interaction. Second level of the
    /// hierarchical partition key.
    public string ConversationId { get; set; } = default!;

    /// 1-based ordinal within the conversation. Single-shot calls always have <c>turnNumber = 1</c>.
    public int TurnNumber { get; set; }

    public LlmConversationPurpose Purpose { get; set; }

    /// Optional link to SQL <c>Documents.ExternalId</c> when the call concerns a specific document.
    public string? DocumentExternalId { get; set; }

    /// Optional link to SQL <c>Uploads.ExternalId</c> for batch-level grouping.
    public string? UploadExternalId { get; set; }

    public string Model { get; set; } = default!;
    public string? ModelVersion { get; set; }

    public LlmTurnInput Input { get; set; } = new();
    public LlmTurnOutput Output { get; set; } = new();
    public LlmTokenCounts Tokens { get; set; } = new();

    public int LatencyMs { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }

    /// Sanitized error message on failure; null on success. Never include secrets or stack traces.
    public string? ErrorMessage { get; set; }
}
