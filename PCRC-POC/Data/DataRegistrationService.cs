using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PCRC.Data.Cosmos;
using PCRC.Data.Repositories;
using PCRC.DataInterface.Configuration;
using PCRC.DataInterface.RepositoryInterfaces;

namespace PCRC.Data;

public static class DataRegistrationService
{
    /// Registers PcrcDbContext (against the named connection string) and every SQL repository.
    /// Call from Program.cs after configuration is bound. Consumers must also register an
    /// <c>IUserContext</c> implementation; the audited repositories depend on it.
    public static IServiceCollection AddPcrcDataLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "PcrcDb")
    {
        services.AddDbContext<PcrcDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(connectionStringName)));

        return services.AddPcrcRepositories();
    }

    /// SQL-repository-only overload for unit tests / callers that wire DbContext themselves.
    public static IServiceCollection AddPcrcRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserClientAccessRepository, UserClientAccessRepository>();
        services.AddScoped<IUploadRepository, UploadRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentMappingTemplateRepository, PaymentMappingTemplateRepository>();
        return services;
    }

    /// Registers a singleton <see cref="CosmosClient"/> bound to the configured account, plus the
    /// Cosmos-backed repositories (<see cref="IAnalyzerResultRepository"/>,
    /// <see cref="ILlmConversationTurnRepository"/>). The client uses System.Text.Json with
    /// camelCase property names to match DataModelCosmos.md.
    public static IServiceCollection AddPcrcCosmosDataLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = CosmosOptions.SectionName)
    {
        services.Configure<CosmosOptions>(configuration.GetSection(sectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.EndpointUri))
                throw new InvalidOperationException($"{sectionName}:EndpointUri must be configured.");
            if (string.IsNullOrWhiteSpace(options.AccountKey))
                throw new InvalidOperationException(
                    $"{sectionName}:AccountKey must be configured. Token-credential auth (DefaultAzureCredential) is a follow-up; add Azure.Identity and swap the factory when needed.");

            var clientOptions = new CosmosClientOptions
            {
                Serializer = PcrcCosmosSerialization.BuildCosmosSerializer(),
                ConsistencyLevel = ConsistencyLevel.Session,
                ApplicationName = "PCRC-POC",
            };
            return new CosmosClient(options.EndpointUri, options.AccountKey, clientOptions);
        });

        services.AddScoped<IAnalyzerResultRepository, AnalyzerResultRepository>();
        services.AddScoped<ILlmConversationTurnRepository, LlmConversationTurnRepository>();
        return services;
    }
}
