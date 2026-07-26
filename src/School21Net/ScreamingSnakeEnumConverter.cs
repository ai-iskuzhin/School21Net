using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace School21Net;

/// <summary>
/// Converts between C# enum members (PascalCase) and the API's <c>SCREAMING_SNAKE_CASE</c> wire values —
/// e.g. <c>InReviews</c> ⇄ <c>IN_REVIEWS</c>. Reading an unrecognised value throws a descriptive
/// <see cref="JsonException"/> rather than silently mapping to 0, so new server values surface loudly.
/// </summary>
internal sealed class ScreamingSnakeEnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private static readonly Dictionary<string, T> FromWire = BuildLookup();

    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        if (raw is not null && FromWire.TryGetValue(Normalize(raw), out var value))
        {
            return value;
        }

        throw School21WireParsing.UnknownEnumValue(typeof(T).Name, raw);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(ToWire(value));

    /// <summary>Render an enum value as its <c>SCREAMING_SNAKE_CASE</c> wire form (also used for query params).</summary>
    public static string ToWire(T value) => ToScreamingSnake(value.ToString());

    private static Dictionary<string, T> BuildLookup()
    {
        var map = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var value in Enum.GetValues<T>())
        {
            map[Normalize(value.ToString())] = value;
        }

        return map;
    }

    // Case- and underscore-insensitive key: "IN_REVIEWS" and "InReviews" both normalize to "INREVIEWS".
    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch != '_')
            {
                builder.Append(char.ToUpperInvariant(ch));
            }
        }

        return builder.ToString();
    }

    private static string ToScreamingSnake(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (i > 0 && char.IsUpper(ch))
            {
                builder.Append('_');
            }

            builder.Append(char.ToUpperInvariant(ch));
        }

        return builder.ToString();
    }
}
