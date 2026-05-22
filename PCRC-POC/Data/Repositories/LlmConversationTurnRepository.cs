using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PCRC.Data.Cosmos;
using PCRC.DataInterface.Configuration;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Cosmos;

namespace PCRC.Data.Repositories;

public class LlmConversationTurnRepository : CosmosRepositoryBase, ILlmConversationTurnRepository
{
    public LlmConversationTurnRepository(CosmosClient client, IOptions<CosmosOptions> options)
        : base(client, options.Value.DatabaseName, options.Value.LlmConversationTurnsContainerName)
    {
    }

    public async Task<LlmConversationTurn> CreateAsync(LlmConversationTurn turn, CancellationToken cancellationToken = default)
    {
        var pk = new PartitionKeyBuilder()
            .Add(turn.ClientId)
            .Add(turn.ConversationId)
            .Build();
        var resp = await Container.CreateItemAsync(turn, pk, cancellationToken: cancellationToken);
        return resp.Resource;
    }

    public Task<List<LlmConversationTurn>> GetConversationAsync(
        Guid clientExternalId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var pk = new PartitionKeyBuilder()
            .Add(clientExternalId.ToString())
            .Add(conversationId.ToString())
            .Build();
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.turnNumber ASC");
        var iterator = Container.GetItemQueryIterator<LlmConversationTurn>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = pk });
        return DrainAsync(iterator, cancellationToken);
    }

    public Task<List<LlmConversationTurn>> GetByDocumentAsync(
        Guid clientExternalId,
        Guid documentExternalId,
        LlmConversationPurpose? purpose = null,
        CancellationToken cancellationToken = default)
    {
        var query = purpose is null
            ? new QueryDefinition(
                "SELECT * FROM c WHERE c.documentExternalId = @docId ORDER BY c.startedAt ASC")
                .WithParameter("@docId", documentExternalId.ToString())
            : new QueryDefinition(
                "SELECT * FROM c WHERE c.documentExternalId = @docId AND c.purpose = @purpose ORDER BY c.startedAt ASC")
                .WithParameter("@docId", documentExternalId.ToString())
                .WithParameter("@purpose", purpose.Value.ToString());

        // Partial hierarchical PK — routes only to the client's physical partitions.
        var pk = new PartitionKey(clientExternalId.ToString());
        var iterator = Container.GetItemQueryIterator<LlmConversationTurn>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = pk });
        return DrainAsync(iterator, cancellationToken);
    }

    public Task<List<LlmConversationTurn>> GetByUploadAsync(
        Guid clientExternalId,
        Guid uploadExternalId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.uploadExternalId = @uploadId ORDER BY c.startedAt ASC")
            .WithParameter("@uploadId", uploadExternalId.ToString());

        var pk = new PartitionKey(clientExternalId.ToString());
        var iterator = Container.GetItemQueryIterator<LlmConversationTurn>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = pk });
        return DrainAsync(iterator, cancellationToken);
    }

    public async Task DeleteAllForClientAsync(Guid clientExternalId, CancellationToken cancellationToken = default)
    {
        using var resp = await Container.DeleteAllItemsByPartitionKeyStreamAsync(
            new PartitionKey(clientExternalId.ToString()),
            cancellationToken: cancellationToken);

        if (!resp.IsSuccessStatusCode && resp.StatusCode != HttpStatusCode.NotFound)
        {
            throw new CosmosException(
                $"Bulk delete for client {clientExternalId} returned status {resp.StatusCode}.",
                resp.StatusCode,
                subStatusCode: 0,
                activityId: resp.Headers?.ActivityId ?? string.Empty,
                requestCharge: resp.Headers?.RequestCharge ?? 0);
        }
    }
}
