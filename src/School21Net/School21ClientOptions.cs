namespace School21Net;

/// <summary>
/// Configuration for <see cref="School21Client"/>. Auth is intentionally not here: the client asks an
/// <see cref="School21Net.Authentication.ISchool21AccessTokenProvider"/> for a bearer on each request, so the
/// integrator owns credentials, token caching and refresh. See <see cref="School21Net.Authentication.School21AuthClient"/>.
/// </summary>
public sealed class School21ClientOptions
{
    /// <summary>Base URL of the public API. Trailing slash optional.</summary>
    public string BaseUrl { get; init; } = "https://platform.21-school.ru/services/21-school/api";
}
