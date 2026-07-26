namespace School21Net.Authentication;

/// <summary>
/// Endpoint configuration for <see cref="School21AuthClient"/>. Defaults target the documented public-API
/// Keycloak realm and <c>s21-open-api</c> client. No credentials live here — they are passed per call to
/// <see cref="School21AuthClient.AuthenticateAsync"/> so the integrator controls where secrets are stored.
/// </summary>
public sealed class School21AuthOptions
{
    /// <summary>Keycloak token endpoint used for the password and refresh grants.</summary>
    public string TokenEndpoint { get; init; } =
        "https://auth.21-school.ru/auth/realms/EduPowerKeycloak/protocol/openid-connect/token";

    /// <summary>OAuth client id for the public API. Defaults to the documented <c>s21-open-api</c>.</summary>
    public string ClientId { get; init; } = "s21-open-api";
}
