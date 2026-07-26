using System.Text.Json;

namespace School21Net;

/// <summary>Internal helpers for wire parsing and argument validation.</summary>
internal static class School21WireParsing
{
    /// <summary>A JSON error for a wire enum value this SDK version does not know — likely a newly added value.</summary>
    public static JsonException UnknownEnumValue(string kind, string? value)
        => new($"Unknown School 21 {kind} value: '{value ?? "<null>"}'. This may be a new value not yet supported " +
               "by this version of School21Net — please report it so it can be added.");

    /// <summary>Require a non-empty string argument, throwing <see cref="School21ValidationException"/> otherwise.</summary>
    public static string RequireNonEmpty(string? value, string paramName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new School21ValidationException($"'{paramName}' must be a non-empty value.")
            : value!;

    /// <summary>URL-escape a path segment.</summary>
    public static string EscapeSegment(string value) => Uri.EscapeDataString(value);
}
