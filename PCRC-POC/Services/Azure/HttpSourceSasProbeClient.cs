using Microsoft.Extensions.Logging;
using PCRC.ServicesInterface.Storage;

namespace PCRC.Services.Azure;

/// HEAD-probes a customer-supplied container SAS URL to validate it exists and grants read perms
/// before we accept a bulk upload. Adds <c>restype=container</c> to the query if it isn't already
/// there — that's the form that returns container metadata (and exercises read on the container,
/// not on a non-existent blob).
public sealed class HttpSourceSasProbeClient : ISourceSasProbeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpSourceSasProbeClient> _logger;

    public HttpSourceSasProbeClient(HttpClient httpClient, ILogger<HttpSourceSasProbeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> CanReadContainerAsync(string sasUrl, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(sasUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("SAS probe rejected — not an absolute URI: {SasUrl}", sasUrl);
            return false;
        }

        var probeUri = EnsureRestypeContainer(uri);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, probeUri);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode) return true;

            _logger.LogWarning("SAS probe failed with status {Status} for {Host}{Path}.",
                (int)response.StatusCode, uri.Host, uri.AbsolutePath);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "SAS probe network error for {Host}{Path}.", uri.Host, uri.AbsolutePath);
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("SAS probe timed out for {Host}{Path}.", uri.Host, uri.AbsolutePath);
            return false;
        }
    }

    private static Uri EnsureRestypeContainer(Uri uri)
    {
        var query = uri.Query.TrimStart('?');
        if (query.Contains("restype=container", StringComparison.OrdinalIgnoreCase))
            return uri;

        var prefix = string.IsNullOrEmpty(query) ? "" : query + "&";
        return new UriBuilder(uri) { Query = prefix + "restype=container" }.Uri;
    }
}
