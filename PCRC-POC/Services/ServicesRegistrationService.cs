using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PCRC.Services.Azure;
using PCRC.Services.Clients;
using PCRC.Services.Documents;
using PCRC.Services.Excel;
using PCRC.Services.Uploads;
using PCRC.ServicesInterface.Clients;
using PCRC.ServicesInterface.Configuration;
using PCRC.ServicesInterface.Documents;
using PCRC.ServicesInterface.Messaging;
using PCRC.ServicesInterface.Storage;
using PCRC.ServicesInterface.Uploads;

namespace PCRC.Services;

public static class ServicesRegistrationService
{
    /// Registers the upload service and the background orphan sweeper for the two-phase direct
    /// flow. Caller must also register the storage adapters (<see cref="AddPcrcAzureStorageAdapters"/>)
    /// and an <c>IUserContext</c>.
    public static IServiceCollection AddPcrcUploadServices(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddScoped<IUploadService, UploadService>();

        if (configuration is not null)
            services.Configure<DirectUploadOrphanSweeperOptions>(
                configuration.GetSection(DirectUploadOrphanSweeperOptions.SectionName));
        else
            services.AddOptions<DirectUploadOrphanSweeperOptions>();

        services.AddHostedService<DirectUploadOrphanSweeper>();
        return services;
    }

    /// Registers the read-side query services for the Client and Document controllers. Depends on
    /// the SQL repositories (<see cref="AddPcrcDataLayer"/>) and an <c>IUserContext</c>.
    public static IServiceCollection AddPcrcQueryServices(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }

    /// Registers <see cref="BlobServiceClient"/>, <see cref="QueueServiceClient"/>, and the
    /// adapters that talk to them. The same code runs against Azurite (default Storage:ConnectionString
    /// = "UseDevelopmentStorage=true") and against a real Azure Storage account when the connection
    /// string is overridden via configuration / Key Vault.
    public static IServiceCollection AddPcrcAzureStorageAdapters(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = StorageOptions.SectionName)
    {
        services.Configure<StorageOptions>(configuration.GetSection(sectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            return new QueueServiceClient(options.ConnectionString);
        });

        services.AddSingleton<IBlobStorageClient, AzureBlobStorageClient>();
        services.AddSingleton<IUploadQueueClient, AzureQueueClient>();

        services.AddHttpClient<ISourceSasProbeClient, HttpSourceSasProbeClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IExcelHeaderReader, ClosedXmlExcelHeaderReader>();
        return services;
    }
}
