using System.Text.Json;

namespace PCRC.Model.Cosmos;

/// One row in the <c>analyzerResults</c> container — Content Understanding output for a Contract
/// Document. Hierarchical partition key is <c>(clientId, md5)</c>; document <c>id</c> equals
/// <c>md5</c> so reads are 1-RU point lookups.
public class AnalyzerResult
{
    /// Lowercase-hex MD5 of the source blob. Equal to <see cref="Md5"/>; duplicated as <c>id</c> so
    /// Cosmos point reads work without a query.
    public string Id { get; set; } = default!;

    /// SQL <c>Clients.ExternalId</c> as a string UUID. First level of the hierarchical partition key.
    public string ClientId { get; set; } = default!;

    /// Lowercase-hex MD5 of the source blob. Second level of the hierarchical partition key.
    public string Md5 { get; set; } = default!;

    public string AnalyzerId { get; set; } = default!;
    public string AnalyzerVersion { get; set; } = default!;
    public string CuRequestId { get; set; } = default!;

    public DateTime ProcessedAt { get; set; }

    /// The raw Content Understanding payload. Shape varies by <see cref="AnalyzerId"/>; held as a
    /// <see cref="JsonElement"/> so we don't bind to a specific analyzer schema in this layer.
    public JsonElement Result { get; set; }
}
