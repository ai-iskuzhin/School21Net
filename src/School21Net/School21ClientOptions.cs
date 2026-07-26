namespace School21Net;

/// <summary>
/// Configuration for <see cref="School21Client"/>. Authenticates to the official School 21 public API via
/// the ROPC password grant (<c>client_id=s21-open-api</c>) using a participant's own credentials.
/// </summary>
/// <remarks>
/// Keep <see cref="Password"/> in secrets, never in source. Note the token's ~10h session ceiling: the
/// client refreshes transparently within it and re-authenticates with the credentials afterwards.
/// </remarks>
public sealed class School21ClientOptions
{
    /// <summary>Base URL of the public API. Trailing slash optional.</summary>
    public string BaseUrl { get; init; } = "https://platform.21-school.ru/services/21-school/api";

    /// <summary>Keycloak token endpoint used for the password grant.</summary>
    public string TokenEndpoint { get; init; } =
        "https://auth.21-school.ru/auth/realms/EduPowerKeycloak/protocol/openid-connect/token";

    /// <summary>OAuth client id for the public API. Defaults to the documented <c>s21-open-api</c>.</summary>
    public string ClientId { get; init; } = "s21-open-api";

    /// <summary>Participant login used to authenticate (the platform account).</summary>
    public required string Username { get; init; }

    /// <summary>Participant password. Store in secrets/env, never in source.</summary>
    public required string Password { get; init; }

    /// <summary>Refresh the cached token this many seconds before it actually expires.</summary>
    public int TokenRefreshSkewSeconds { get; init; } = 60;
}
