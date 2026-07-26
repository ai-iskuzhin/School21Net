using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using School21Net.Resources;

namespace School21Net;

/// <summary>
/// Typed client for the official School 21 public API (<c>platform.21-school.ru/services/21-school/api</c>).
/// Authenticates with the ROPC password grant and caches/refreshes the bearer transparently. Endpoints are
/// grouped into <see cref="Participants"/>, <see cref="Projects"/>, <see cref="Campuses"/> and
/// <see cref="Coalitions"/>. Non-2xx responses throw <see cref="School21ApiException"/>.
/// </summary>
/// <example>
/// <code>
/// var client = new School21Client(httpClient, new School21ClientOptions { Username = "login", Password = "***" });
/// var finishers = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);
/// </code>
/// </example>
public sealed class School21Client
{
    internal static readonly JsonSerializerOptions Json = CreateJsonOptions();
    private static readonly string UserAgent = BuildUserAgent();

    private readonly HttpClient _http;
    private readonly School21ClientOptions _options;
    private readonly string _baseUrl;
    private readonly SemaphoreSlim _tokenGate = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAt = DateTimeOffset.MinValue;

    /// <summary>Create a client. Pass a pooled <paramref name="httpClient"/> (e.g. from <c>IHttpClientFactory</c>).</summary>
    /// <exception cref="ArgumentNullException">If an argument is null.</exception>
    /// <exception cref="School21ValidationException">If credentials are missing.</exception>
    public School21Client(HttpClient httpClient, School21ClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        School21WireParsing.RequireNonEmpty(options.Username, nameof(options.Username));
        School21WireParsing.RequireNonEmpty(options.Password, nameof(options.Password));
        School21WireParsing.RequireNonEmpty(options.BaseUrl, nameof(options.BaseUrl));
        School21WireParsing.RequireNonEmpty(options.TokenEndpoint, nameof(options.TokenEndpoint));

        _http = httpClient;
        _options = options;
        _baseUrl = options.BaseUrl.TrimEnd('/');

        Participants = new ParticipantsResource(this);
        Projects = new ProjectsResource(this);
        Campuses = new CampusesResource(this);
        Coalitions = new CoalitionsResource(this);
    }

    /// <summary>Participant-scoped endpoints (basic info, projects, coalition, points, feedback).</summary>
    public ParticipantsResource Participants { get; }

    /// <summary>Project-scoped endpoints (per-project participant lists, filterable by status).</summary>
    public ProjectsResource Projects { get; }

    /// <summary>Campus-scoped endpoints (list campuses, campus participants and coalitions).</summary>
    public CampusesResource Campuses { get; }

    /// <summary>Coalition-scoped endpoints (coalition participant lists).</summary>
    public CoalitionsResource Coalitions { get; }

    /// <summary>GET a single typed object from the API.</summary>
    internal async Task<T> GetAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + relativePath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.ParseAdd(UserAgent);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new School21TransportException($"School 21 GET {relativePath} failed before a response.", exception);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw CreateApiException(relativePath, response.StatusCode, body);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new School21ProtocolException($"School 21 GET {relativePath} returned an empty body.", response.StatusCode);
            }

            try
            {
                return JsonSerializer.Deserialize<T>(body, Json)
                    ?? throw new School21ProtocolException($"School 21 GET {relativePath} deserialized to null.", response.StatusCode);
            }
            catch (JsonException exception)
            {
                throw new School21ProtocolException(
                    $"School 21 GET {relativePath} returned JSON that did not match the expected model.",
                    response.StatusCode,
                    exception);
            }
        }
    }

    /// <summary>GET every page of a list endpoint, following <c>limit</c>/<c>offset</c> until a short page.</summary>
    internal async Task<IReadOnlyList<TItem>> GetPagedAsync<TEnvelope, TItem>(
        string relativePath,
        Func<TEnvelope, IReadOnlyList<TItem>?> selector,
        IReadOnlyList<KeyValuePair<string, string>>? query,
        CancellationToken cancellationToken)
    {
        const int pageSize = 1000;
        var all = new List<TItem>();
        var offset = 0;

        while (true)
        {
            var pageQuery = new List<KeyValuePair<string, string>>(query ?? [])
            {
                new("limit", pageSize.ToString()),
                new("offset", offset.ToString())
            };

            var envelope = await GetAsync<TEnvelope>(relativePath + BuildQueryString(pageQuery), cancellationToken)
                .ConfigureAwait(false);
            var items = selector(envelope);
            if (items is null || items.Count == 0)
            {
                break;
            }

            all.AddRange(items);
            if (items.Count < pageSize)
            {
                break;
            }

            offset += items.Count;
        }

        return all;
    }

    internal static string BuildQueryString(IReadOnlyList<KeyValuePair<string, string>> query)
    {
        if (query.Count == 0)
        {
            return string.Empty;
        }

        var parts = query.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
        return "?" + string.Join("&", parts);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (IsTokenFresh())
        {
            return _accessToken!;
        }

        await _tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsTokenFresh())
            {
                return _accessToken!;
            }

            var form = new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("username", _options.Username),
                new KeyValuePair<string, string>("password", _options.Password)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };
            request.Headers.UserAgent.ParseAdd(UserAgent);

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
                    throw CreateApiException("<token>", response.StatusCode, body);
                }

                TokenResponse? token;
                try
                {
                    token = JsonSerializer.Deserialize<TokenResponse>(body, Json);
                }
                catch (JsonException exception)
                {
                    throw new School21ProtocolException("School 21 token response was not valid JSON.", response.StatusCode, exception);
                }

                if (token is null || string.IsNullOrEmpty(token.AccessToken))
                {
                    throw new School21ProtocolException("School 21 token response contained no access_token.", response.StatusCode);
                }

                _accessToken = token.AccessToken;
                var lifetime = Math.Max(30, token.ExpiresIn - _options.TokenRefreshSkewSeconds);
                _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime);
                return _accessToken;
            }
        }
        finally
        {
            _tokenGate.Release();
        }
    }

    private bool IsTokenFresh() => _accessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpiresAt;

    private static School21ApiException CreateApiException(string path, System.Net.HttpStatusCode statusCode, string body)
    {
        School21Error? error = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                error = JsonSerializer.Deserialize<School21Error>(body, Json);
            }
            catch (JsonException)
            {
                // Non-JSON error body — leave error null.
            }
        }

        var detail = error?.ErrorDescription ?? error?.Message;
        return new School21ApiException(
            statusCode,
            error?.Error,
            detail,
            $"School 21 GET {path} returned HTTP {(int)statusCode} ({statusCode}){(detail is null ? "." : $": {detail}")}");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        // REST responses are camelCase (Web defaults). Relaxed encoder keeps Cyrillic readable.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new ScreamingSnakeEnumConverter<ParticipantStatus>());
        options.Converters.Add(new ScreamingSnakeEnumConverter<ParticipantProjectType>());
        options.Converters.Add(new ScreamingSnakeEnumConverter<ParticipantProjectStatus>());
        return options;
    }

    private static string BuildUserAgent()
    {
        var assembly = typeof(School21Client).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";

        var plus = version.IndexOf('+');
        if (plus >= 0)
        {
            version = version.Substring(0, plus);
        }

        return $"School21Net/{version} ({RuntimeInformation.FrameworkDescription})";
    }
}
