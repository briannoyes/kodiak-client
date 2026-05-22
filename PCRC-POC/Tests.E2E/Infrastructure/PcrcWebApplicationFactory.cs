using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PCRC.Tests.E2E.Infrastructure;

/// WebApplicationFactory that points the running Api at:
///   - a unique SQL database per fixture instance (created in <see cref="DatabaseFixture"/>
///     before the factory is used, dropped on dispose),
///   - a unique Azure Blob container in the configured Storage account (Azurite by default),
///   - the orphan sweeper disabled so tests drive it explicitly.
///
/// SQL connection: override the default by setting <c>PCRC_TESTS_SQL_CONN_STRING_TEMPLATE</c>
/// (use the literal token <c>{DB}</c> where the database name should be substituted). The default
/// targets <c>localhost,1433</c> with the standard SA credentials used by the mssql/server image.
/// Storage connection: override via <c>PCRC_TESTS_STORAGE_CONN_STRING</c>; defaults to Azurite
/// (<c>UseDevelopmentStorage=true</c>).
public sealed class PcrcWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; }
    public string BlobContainerName { get; }
    public string SqlConnectionString { get; }
    public string StorageConnectionString { get; }

    public PcrcWebApplicationFactory()
    {
        var runId = Guid.NewGuid().ToString("N");
        DatabaseName = $"PcrcTests_{runId}";
        BlobContainerName = $"uploads-tests-{runId}";

        var template = Environment.GetEnvironmentVariable("PCRC_TESTS_SQL_CONN_STRING_TEMPLATE")
            ?? "Server=localhost,1433;Database={DB};User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;";
        SqlConnectionString = template.Replace("{DB}", DatabaseName);

        StorageConnectionString = Environment.GetEnvironmentVariable("PCRC_TESTS_STORAGE_CONN_STRING")
            ?? "UseDevelopmentStorage=true";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PcrcDb"] = SqlConnectionString,
                ["Storage:ConnectionString"] = StorageConnectionString,
                ["Storage:BlobContainerName"] = BlobContainerName,
                ["Storage:AutoCreateQueues"] = "true",
                // CosmosClient is registered as a singleton but lazily constructed; the upload paths
                // under test never resolve it. We still set valid-looking values to keep startup
                // happy if anything ever does touch the option object.
                ["Cosmos:EndpointUri"] = "https://localhost:8081/",
                ["Cosmos:AccountKey"] = "FAKE_ACCOUNT_KEY==",
                ["Cosmos:DatabaseName"] = "pcrc-tests",
                ["Uploads:DirectOrphanSweeper:Enabled"] = "false",
            });
        });
    }
}