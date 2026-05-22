using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCRC.ServicesInterface.Configuration;
using PCRC.ServicesInterface.Messaging;

namespace PCRC.Services.Azure;

public sealed class AzureQueueClient : IUploadQueueClient
{
    private readonly QueueServiceClient _service;
    private readonly StorageOptions _options;
    private readonly ILogger<AzureQueueClient> _logger;
    private readonly ConcurrentDictionary<string, Lazy<Task<QueueClient>>> _clients = new();

    public AzureQueueClient(
        QueueServiceClient service,
        IOptions<StorageOptions> options,
        ILogger<AzureQueueClient> logger)
    {
        _service = service;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnqueueAsync<T>(string queueName, T message, CancellationToken cancellationToken)
    {
        var client = await _clients
            .GetOrAdd(queueName, name => new Lazy<Task<QueueClient>>(() => GetOrCreateQueueAsync(name, cancellationToken)))
            .Value;

        var payload = JsonSerializer.Serialize(message);
        await client.SendMessageAsync(payload, cancellationToken);
        _logger.LogDebug("Enqueued {Bytes} bytes to queue {Queue}.", payload.Length, queueName);
    }

    private async Task<QueueClient> GetOrCreateQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        var client = _service.GetQueueClient(queueName);
        if (_options.AutoCreateQueues)
        {
            await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        }
        return client;
    }
}
