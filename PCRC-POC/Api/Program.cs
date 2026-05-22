using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PCRC.Api.Infrastructure;
using PCRC.Data;
using PCRC.DataInterface;
using PCRC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

const string LocalhostCorsPolicy = "LocalhostDev";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LocalhostCorsPolicy, policy => policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
    });
}

builder.Services.AddScoped<IUserContext, HeaderUserContext>();
builder.Services.AddPcrcDataLayer(builder.Configuration);
builder.Services.AddPcrcCosmosDataLayer(builder.Configuration);
builder.Services.AddPcrcUploadServices(builder.Configuration);
builder.Services.AddPcrcQueryServices();
builder.Services.AddPcrcAzureStorageAdapters(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(LocalhostCorsPolicy);
    await ConfigureBlobCorsForDevAsync(app);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

static async Task ConfigureBlobCorsForDevAsync(WebApplication app)
{
    const string Origin = "http://localhost:4200";
    try
    {
        var blob = app.Services.GetRequiredService<BlobServiceClient>();
        var props = (await blob.GetPropertiesAsync()).Value;
        if (props.Cors.Any(r => r.AllowedOrigins?.Contains(Origin) == true)) return;
        props.Cors.Add(new BlobCorsRule
        {
            AllowedOrigins = Origin,
            AllowedMethods = "GET,PUT,OPTIONS,HEAD",
            AllowedHeaders = "*",
            ExposedHeaders = "*",
            MaxAgeInSeconds = 3600,
        });
        await blob.SetPropertiesAsync(props);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "Could not configure Blob CORS for {Origin}. Browser-direct uploads to Azurite may be blocked until CORS is set.",
            Origin);
    }
}

/// Exposed so the E2E test project (Tests.E2E) can use WebApplicationFactory&lt;Program&gt;.
public partial class Program;
