using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PCRC.Data;
using PCRC.Model.Authorization;
using PCRC.Model.Clients;
using PCRC.Model.Users;
using Xunit;

namespace PCRC.Tests.E2E.Infrastructure;

/// xUnit collection fixture that owns one <see cref="PcrcWebApplicationFactory"/>, brings up a
/// per-run SQL database via EF Core's <c>EnsureCreated</c>, seeds a Client + User with a
/// UserClientAccess grant, and tears everything down at the end of the test session.
///
/// Requires:
///   - SQL Server reachable at the configured connection string (Docker: `mcr.microsoft.com/mssql/server`,
///     port 1433, SA password matching the default template — or set
///     <c>PCRC_TESTS_SQL_CONN_STRING_TEMPLATE</c>).
///   - Azurite reachable at <c>UseDevelopmentStorage=true</c> (or set <c>PCRC_TESTS_STORAGE_CONN_STRING</c>).
public sealed class DatabaseFixture : IAsyncLifetime
{
    public PcrcWebApplicationFactory Factory { get; }

    public Guid SeededClientExternalId { get; private set; }
    public long SeededUserId { get; private set; }
    public string SeededUserEntraObjectId { get; private set; } = default!;

    public DatabaseFixture()
    {
        Factory = new PcrcWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        // Touching Factory.Services boots the host, which is what we want — Program has now read
        // the in-memory test configuration so the DbContext is bound to our unique database.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PcrcDbContext>();

        await db.Database.EnsureCreatedAsync();

        var user = new User
        {
            ExternalId = Guid.NewGuid(),
            EntraObjectId = $"entra-{Guid.NewGuid():N}",
            Email = $"e2e-{Guid.NewGuid():N}@tests.local",
            DisplayName = "E2E Test User",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };
        db.Users.Add(user);

        var client = new Client
        {
            ExternalId = Guid.NewGuid(),
            Name = "E2E Test Client",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        db.UserClientAccess.Add(new UserClientAccess
        {
            ExternalId = Guid.NewGuid(),
            UserId = user.Id,
            ClientId = client.Id,
            Status = UserClientAccessStatus.Active,
            GrantedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        SeededUserId = user.Id;
        SeededUserEntraObjectId = user.EntraObjectId;
        SeededClientExternalId = client.ExternalId;
    }

    public async Task DisposeAsync()
    {
        try
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PcrcDbContext>();
            await db.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Best effort — don't fail the suite if the drop hiccups; the unique DB name keeps
            // reruns clean either way.
        }

        try
        {
            var blobService = new BlobServiceClient(Factory.StorageConnectionString);
            await blobService.DeleteBlobContainerAsync(Factory.BlobContainerName);
        }
        catch
        {
            // Best effort.
        }

        await Factory.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "PcrcDatabase";
}