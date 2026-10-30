using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GTFO.API.JSON.Converters;

internal sealed class AssetTagsConverter : JsonConverter<IReadOnlySet<string>>
{
    public override IReadOnlySet<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tagsArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(ref reader, options);

        if (tagsArray == null)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return new HashSet<string>(tagsArray.Where(tag => !string.IsNullOrWhiteSpace(tag)), StringComparer.OrdinalIgnoreCase);
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlySet<string> value, JsonSerializerOptions options)
    {
        System.Text.Json.JsonSerializer.Serialize(writer, value.ToArray(), options);
    }
}
