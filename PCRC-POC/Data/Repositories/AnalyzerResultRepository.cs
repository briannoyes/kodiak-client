using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PCRC.Data.Cosmos;
using PCRC.DataInterface.Configuration;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Cosmos;

namespace PCRC.Data.Repositories;

public class AnalyzerResultRepository : CosmosRepositoryBase, IAnalyzerResultRepository
{
    public AnalyzerResultRepository(CosmosClient client, IOptions<CosmosOptions> options)
        : base(client, options.Value.DatabaseName, options.Value.AnalyzerResultsContainerName)
    {
    }

    public Task<AnalyzerResult?> GetAsync(Guid clientExternalId, string md5, CancellationToken cancellationToken = default)
        => ReadOrNullAsync<AnalyzerResult>(md5, BuildPartitionKey(clientExternalId, md5), cancellationToken);

    public async Task<bool> ExistsAsync(Guid clientExternalId, string md5, CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await Container.ReadItemStreamAsync(
                md5,
                BuildPartitionKey(clientExternalId, md5),
                cancellationToken: cancellationToken);
            return resp.IsSuccessStatusCode;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<AnalyzerResult> CreateAsync(AnalyzerResult result, CancellationToken cancellationToken = default)
    {
        var resp = await Container.CreateItemAsync(
            result,
            BuildPartitionKey(result),
            cancellationToken: cancellationToken);
        return resp.Resource;
    }

    public async Task<AnalyzerResult> UpsertAsync(AnalyzerResult result, CancellationToken cancellationToken = default)
    {
        var resp = await Container.UpsertItemAsync(
            result,
            BuildPartitionKey(result),
            cancellationToken: cancellationToken);
        return resp.Resource;
    }

    public async Task DeleteAllForClientAsync(Guid clientExternalId, CancellationToken cancellationToken = default)
    {
        // Partial hierarchical PK: passing only the first level deletes every doc for the client.
        // The archive workflow runs this exactly once per client transition.
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

    private static PartitionKey BuildPartitionKey(AnalyzerResult result)
        => new PartitionKeyBuilder()
            .Add(result.ClientId)
            .Add(result.Md5)
            .Build();

    private static PartitionKey BuildPartitionKey(Guid clientExternalId, string md5)
        => new PartitionKeyBuilder()
            .Add(clientExternalId.ToString())
            .Add(md5)
            .Build();
}
