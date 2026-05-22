namespace PCRC.ServicesInterface.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// Connection string for Azure Blob + Queue Storage. Use <c>UseDevelopmentStorage=true</c> to
    /// target a local Azurite instance; in cloud, supply the real account connection string (or
    /// switch to <c>BlobServiceEndpoint</c>/<c>QueueServiceEndpoint</c> + DefaultAzureCredential
    /// when that follow-up lands).
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";

    /// Container that holds every upload's blobs. Each blob name is
    /// <c>{clientExternalId}/{uploadExternalId}/{fileName}</c>.
    public string BlobContainerName { get; set; } = "uploads";

    /// When true, the queue client lazily calls <c>CreateIfNotExistsAsync</c> on first send. Handy
    /// for Azurite/dev; leave on in cloud too — the call is a no-op once the queue exists.
    public bool AutoCreateQueues { get; set; } = true;
}
