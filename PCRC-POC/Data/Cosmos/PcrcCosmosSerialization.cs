using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

namespace PCRC.Data.Cosmos;

/// Centralized JSON-serialization knobs for everything we read or write through Cosmos. Used by
/// both the singleton <see cref="CosmosClient"/> registration and any direct serializer use
/// (e.g., raw stream operations in repositories).
public static class PcrcCosmosSerialization
{
    public static JsonSerializerOptions BuildJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Enum names ship as PascalCase ("RulesDerivation") to match DataModelCosmos.md, while
        // property names use camelCase ("conversationId") — so the enum converter intentionally
        // skips the camelCase naming policy.
        Converters = { new JsonStringEnumConverter() },
    };

    internal static CosmosSerializer BuildCosmosSerializer()
        => new SystemTextJsonCosmosSerializer(BuildJsonOptions());
}
