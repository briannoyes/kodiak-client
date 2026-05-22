namespace PCRC.ServicesInterface.Messaging;

/// Thin port over Azure Storage Queues. Each message payload is opaque to the producer — it's a
/// JSON envelope shaped to the worker on the other side. Queue name comes from
/// <see cref="UploadQueueNames"/>.
public interface IUploadQueueClient
{
    Task EnqueueAsync<T>(string queueName, T message, CancellationToken cancellationToken);
}
