using PCRC.Model.Cosmos;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface ILlmConversationTurnRepository
{
    /// Writes one turn. Caller is responsible for assigning <c>Id</c> (a UUID) and
    /// <c>TurnNumber</c> before the call.
    Task<LlmConversationTurn> CreateAsync(LlmConversationTurn turn, CancellationToken cancellationToken = default);

    /// Single-partition query: every turn of a conversation, in <c>turnNumber</c> order.
    Task<List<LlmConversationTurn>> GetConversationAsync(
        Guid clientExternalId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    /// Prefix-key query (partition routes only to the client's partitions). Optionally filtered by
    /// <see cref="LlmConversationPurpose"/> when the caller wants e.g. only RulesDerivation turns
    /// for the document.
    Task<List<LlmConversationTurn>> GetByDocumentAsync(
        Guid clientExternalId,
        Guid documentExternalId,
        LlmConversationPurpose? purpose = null,
        CancellationToken cancellationToken = default);

    Task<List<LlmConversationTurn>> GetByUploadAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        CancellationToken cancellationToken = default);

    /// Removes every conversation turn for a client. Run as part of the archive workflow.
    Task DeleteAllForClientAsync(Guid clientExternalId, CancellationToken cancellationToken = default);
}
