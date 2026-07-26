using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using School21Net.Authentication;
using School21Net.Resources;

namespace School21Net;

/// <summary>
/// Typed client for the official School 21 public API (<c>platform.21-school.ru/services/21-school/api</c>).
/// It is auth-agnostic: before every request it asks an <see cref="ISchool21AccessTokenProvider"/> for a
/// bearer, so credentials, token caching and refresh live entirely on the integrator side (obtain tokens
/// with <see cref="School21AuthClient"/>). Endpoints are grouped into <see cref="Participants"/>,
/// <see cref="Projects"/>, <see cref="Campuses"/> and <see cref="Coalitions"/>. Non-2xx responses throw
/// <see cref="School21ApiException"/>.
/// </summary>
/// <example>
/// <code>
/// var auth = new School21AuthClient(httpClient);
/// var token = await auth.AuthenticateAsync("login", "***", ct);
/// var client = new School21Client(httpClient, new School21ClientOptions(), new StaticAccessTokenProvider(token.AccessToken));
/// var finishers = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);
/// </code>
/// </example>
public sealed class School21Client
{
    internal static readonly JsonSerializerOptions Json = CreateJsonOptions();
    internal static readonly string UserAgent = BuildUserAgent();

    private readonly HttpClient _http;
    private readonly ISchool21AccessTokenProvider _accessTokenProvider;
    private readonly string _baseUrl;

    /// <summary>
    /// Create a client. Pass a pooled <paramref name="httpClient"/> (e.g. from <c>IHttpClientFactory</c>) and an
    /// <paramref name="accessTokenProvider"/> that yields a valid bearer (use
    /// <see cref="StaticAccessTokenProvider"/>, <see cref="DelegateAccessTokenProvider"/>, or your own
    /// refresh-aware implementation).
    /// </summary>
    /// <exception cref="ArgumentNullException">If an argument is null.</exception>
    /// <exception cref="School21ValidationException">If the base URL is missing.</exception>
    public School21Client(
        HttpClient httpClient,
        School21ClientOptions options,
        ISchool21AccessTokenProvider accessTokenProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(accessTokenProvider);
        School21WireParsing.RequireNonEmpty(options.BaseUrl, nameof(options.BaseUrl));

        _http = httpClient;
        _accessTokenProvider = accessTokenProvider;
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
        var token = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
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

    internal static School21ApiException CreateApiException(string path, System.Net.HttpStatusCode statusCode, string body)
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
            $"School 21 {path} returned HTTP {(int)statusCode} ({statusCode}){(detail is null ? "." : $": {detail}")}");
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
