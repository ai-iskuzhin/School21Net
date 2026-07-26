using System.Text.Json;
using System.Text.Json.Serialization;

namespace School21Net.Authentication;

/// <summary>
/// Obtains and refreshes School 21 public-API tokens against the Keycloak token endpoint. This is the only
/// place the SDK talks to auth, and it never stores credentials or tokens — each call is stateless and
/// returns a <see cref="School21Token"/>. The integrator decides when to refresh (see
/// <see cref="ISchool21AccessTokenProvider"/>): keep the <see cref="School21Token.RefreshToken"/> and call
/// <see cref="RefreshAsync"/> before <see cref="School21Token.AccessTokenExpiresAtUtc"/>, falling back to
/// <see cref="AuthenticateAsync"/> once the refresh token is gone (~10h session ceiling).
/// </summary>
/// <example>
/// <code>
/// var auth = new School21AuthClient(httpClient);
/// var token = await auth.AuthenticateAsync("login", "***", ct);
/// // ...later, on the integrator side:
/// token = await auth.RefreshAsync(token.RefreshToken!, ct);
/// </code>
/// </example>
public sealed class School21AuthClient
{
    private readonly HttpClient _http;
    private readonly School21AuthOptions _options;

    /// <summary>Create an auth client. Pass a pooled <paramref name="httpClient"/> (e.g. from <c>IHttpClientFactory</c>).</summary>
    /// <exception cref="ArgumentNullException">If <paramref name="httpClient"/> is null.</exception>
    public School21AuthClient(HttpClient httpClient, School21AuthOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _http = httpClient;
        _options = options ?? new School21AuthOptions();
        School21WireParsing.RequireNonEmpty(_options.TokenEndpoint, nameof(_options.TokenEndpoint));
        School21WireParsing.RequireNonEmpty(_options.ClientId, nameof(_options.ClientId));
    }

    /// <summary>Authenticate with a participant's login and password (ROPC password grant).</summary>
    /// <exception cref="School21ValidationException">If credentials are missing.</exception>
    /// <exception cref="School21ApiException">If the token endpoint returns a non-2xx response.</exception>
    /// <exception cref="School21ProtocolException">If the response is not the expected token JSON.</exception>
    /// <exception cref="School21TransportException">If the request fails before a response.</exception>
    public Task<School21Token> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        School21WireParsing.RequireNonEmpty(username, nameof(username));
        School21WireParsing.RequireNonEmpty(password, nameof(password));

        return RequestTokenAsync(
            new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            },
            cancellationToken);
    }

    /// <summary>Exchange a refresh token for a fresh token set (refresh_token grant).</summary>
    /// <exception cref="School21ValidationException">If <paramref name="refreshToken"/> is missing.</exception>
    /// <exception cref="School21ApiException">If the token endpoint returns a non-2xx response (e.g. an expired refresh token).</exception>
    /// <exception cref="School21ProtocolException">If the response is not the expected token JSON.</exception>
    /// <exception cref="School21TransportException">If the request fails before a response.</exception>
    public Task<School21Token> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        School21WireParsing.RequireNonEmpty(refreshToken, nameof(refreshToken));

        return RequestTokenAsync(
            new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            },
            cancellationToken);
    }

    private async Task<School21Token> RequestTokenAsync(
        IReadOnlyList<KeyValuePair<string, string>> form,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(form)
        };
        request.Headers.UserAgent.ParseAdd(School21Client.UserAgent);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new School21TransportException("School 21 token request failed before a response.", exception);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw School21Client.CreateApiException("<token>", response.StatusCode, body);
            }

            TokenWireResponse? wire;
            try
            {
                wire = JsonSerializer.Deserialize<TokenWireResponse>(body, School21Client.Json);
            }
            catch (JsonException exception)
            {
                throw new School21ProtocolException("School 21 token response was not valid JSON.", response.StatusCode, exception);
            }

            if (wire is null || string.IsNullOrEmpty(wire.AccessToken))
            {
                throw new School21ProtocolException("School 21 token response contained no access_token.", response.StatusCode);
            }

            return new School21Token
            {
                AccessToken = wire.AccessToken,
                RefreshToken = wire.RefreshToken,
                ExpiresIn = wire.ExpiresIn,
                RefreshExpiresIn = wire.RefreshExpiresIn,
                TokenType = wire.TokenType,
                ObtainedAtUtc = DateTimeOffset.UtcNow
            };
        }
    }

    private sealed record TokenWireResponse
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("refresh_expires_in")] public int RefreshExpiresIn { get; init; }
        [JsonPropertyName("token_type")] public string? TokenType { get; init; }
    }
}
