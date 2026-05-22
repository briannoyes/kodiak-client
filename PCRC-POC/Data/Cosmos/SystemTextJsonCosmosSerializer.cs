using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace PCRC.Data.Cosmos;

/// Wraps <see cref="System.Text.Json.JsonSerializer"/> as a Cosmos <see cref="CosmosSerializer"/>.
/// Lets us keep the Model project free of Newtonsoft.Json (the SDK's default) without depending
/// on the SDK's <c>UseSystemTextJsonSerializerWithOptions</c> shortcut, which has shifted across
/// SDK versions.
internal sealed class SystemTextJsonCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonCosmosSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }
            return JsonSerializer.Deserialize<T>(stream, _options)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var ms = new MemoryStream();
        JsonSerializer.Serialize(ms, input, _options);
        ms.Position = 0;
        return ms;
    }
}
