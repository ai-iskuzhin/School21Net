using System.Text.Json;
using System.Text.Json.Serialization;

namespace School21Net;

/// <summary>
/// Supplies a <see cref="ScreamingSnakeEnumConverter{T}"/> for every enum the models use, and a
/// tolerant one for every nullable enum.
/// <para>
/// A factory rather than a list of registrations, because the list was a trap. Each enum had to be
/// named in <see cref="School21Client"/>, and forgetting one compiled, passed every test that did
/// not exercise that field, and then threw the first time the real API sent that value. It is not a
/// mistake anybody makes twice, but it is one everybody makes once, and there is no reason the
/// compiler should not do the remembering.
/// </para>
/// </summary>
internal sealed class ScreamingSnakeEnumConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => Target(typeToConvert).IsEnum;

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var target = Target(typeToConvert);

        // Nullable gets the tolerant reader, non-nullable the strict one. The difference is not a
        // preference: a property typed `Status?` can express "the school said something I do not
        // know", and one typed `Status` cannot, so only the first has anywhere to put an unknown.
        var converter = Nullable.GetUnderlyingType(typeToConvert) is null
            ? typeof(ScreamingSnakeEnumConverter<>).MakeGenericType(target)
            : typeof(TolerantScreamingSnakeEnumConverter<>).MakeGenericType(target);

        return (JsonConverter)Activator.CreateInstance(converter)!;
    }

    private static Type Target(Type type) => Nullable.GetUnderlyingType(type) ?? type;
}

/// <summary>
/// Reads a <c>SCREAMING_SNAKE_CASE</c> enum, answering <c>null</c> for a value this version does not
/// know.
/// <para>
/// <b>Unknown values must not throw here.</b> These enums describe somebody else's vocabulary, and it
/// grows without asking us. A strict read turns "the school added a status" into a total failure of
/// every call whose response happens to contain it — one new participant status would stop every
/// profile sync, for every member, including the ones whose status has not changed. A null in one
/// field is a smaller loss than that by any measure.
/// </para>
/// <para>
/// The raw string is not preserved, and that is a real cost worth naming: a caller learns that the
/// value was unrecognised, not what it was. Keeping it would mean a companion property on every
/// model — worth doing the day somebody needs to act on an unknown value, and not before.
/// </para>
/// </summary>
internal sealed class TolerantScreamingSnakeEnumConverter<T> : JsonConverter<T?>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return ScreamingSnakeEnumConverter<T>.TryParse(reader.GetString(), out var value) ? value : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is { } present)
        {
            writer.WriteStringValue(ScreamingSnakeEnumConverter<T>.ToWire(present));
            return;
        }

        writer.WriteNullValue();
    }
}
