using System.Net;
using System.Text.Json.Serialization;

namespace School21Net;

/// <summary>Base type for every error raised by <see cref="School21Client"/>.</summary>
public abstract class School21Exception : Exception
{
    /// <summary>Create the exception.</summary>
    protected School21Exception(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>The request never produced a response (DNS, TLS, connection, timeout).</summary>
public sealed class School21TransportException : School21Exception
{
    /// <summary>Create the exception.</summary>
    public School21TransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The API returned a non-2xx status. Carries the <see cref="StatusCode"/> and parsed error body.</summary>
public sealed class School21ApiException : School21Exception
{
    /// <summary>The HTTP status returned.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>True for HTTP 401/403.</summary>
    public bool IsAuthError => (int)StatusCode is 401 or 403;

    /// <summary>The machine-readable error code from the body, if any (e.g. OAuth <c>error</c>).</summary>
    public string? ErrorCode { get; }

    /// <summary>The human-readable error text from the body, if any.</summary>
    public string? Detail { get; }

    /// <summary>Create the exception.</summary>
    public School21ApiException(HttpStatusCode statusCode, string? errorCode, string? detail, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Detail = detail;
    }
}

/// <summary>A 2xx response arrived but could not be parsed into the expected model (or an unknown enum value).</summary>
public sealed class School21ProtocolException : School21Exception
{
    /// <summary>The HTTP status of the response, if known.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Create the exception.</summary>
    public School21ProtocolException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>A caller passed an invalid argument (empty login, missing credentials).</summary>
public sealed class School21ValidationException : School21Exception
{
    /// <summary>Create the exception.</summary>
    public School21ValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>The OAuth/error envelope returned by the auth or API layer.</summary>
public sealed record School21Error
{
    /// <summary>OAuth <c>error</c> code, or an API error code.</summary>
    [JsonPropertyName("error")] public string? Error { get; init; }

    /// <summary>OAuth <c>error_description</c>.</summary>
    [JsonPropertyName("error_description")] public string? ErrorDescription { get; init; }

    /// <summary>API <c>message</c>, when present instead of the OAuth shape.</summary>
    [JsonPropertyName("message")] public string? Message { get; init; }
}
