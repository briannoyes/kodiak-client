using System.Net;
using Microsoft.Azure.Cosmos;

namespace PCRC.Data.Cosmos;

/// Shared scaffolding for Cosmos-backed repositories: container resolution, NotFound→null point
/// reads, and feed-iterator drainage. Subclasses are thin wrappers that bind one container and
/// expose entity-specific query methods.
public abstract class CosmosRepositoryBase
{
    protected readonly Container Container;

    protected CosmosRepositoryBase(CosmosClient client, string databaseName, string containerName)
    {
        Container = client.GetContainer(databaseName, containerName);
    }

    protected static async Task<List<T>> DrainAsync<T>(
        FeedIterator<T> iterator,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(page);
        }
        return results;
    }

    protected async Task<T?> ReadOrNullAsync<T>(
        string id,
        PartitionKey partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await Container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }
}
