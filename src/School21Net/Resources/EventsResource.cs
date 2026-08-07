using System.Globalization;

namespace School21Net.Resources;

/// <summary>Campus events (<c>GET /v1/events</c>).</summary>
/// <remarks>
/// <para>
/// <b>The window is required.</b> The API rejects a call without <c>from</c> and <c>to</c>, and
/// rejects them unless they carry a zone — a bare <c>2026-08-01</c>, or even
/// <c>2026-08-01T00:00:00</c>, comes back as a bare 400 naming no field. This resource formats them
/// so a caller cannot get that wrong.
/// </para>
/// <para>
/// <b>One campus only.</b> There is no campus parameter: the feed is scoped to the campus of the
/// account the token belongs to. An integrator serving several campuses needs an account in each.
/// </para>
/// </remarks>
public sealed class EventsResource
{
    private readonly School21Client _client;

    internal EventsResource(School21Client client) => _client = client;

    /// <summary>Events starting in a window, oldest first.</summary>
    /// <param name="from">Start of the window, inclusive.</param>
    /// <param name="to">End of the window.</param>
    /// <param name="type">Narrow to one kind, or null for all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<IReadOnlyList<SchoolEvent>> GetAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        EventType? type = null,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "The end of the window is before its start.");
        }

        var query = new List<KeyValuePair<string, string>>
        {
            new("from", Instant(from)),
            new("to", Instant(to))
        };

        if (type is { } wanted)
        {
            query.Add(new KeyValuePair<string, string>("type", ScreamingSnakeEnumConverter<EventType>.ToWire(wanted)));
        }

        return _client.GetPagedAsync<EventsEnvelope, SchoolEvent>(
            "/v1/events",
            envelope => envelope.Events,
            query,
            cancellationToken);
    }

    /// <summary>
    /// UTC with a <c>Z</c>, which is the one shape the API accepts. Converted rather than assumed, so
    /// that a caller passing a local time gets the instant they meant instead of a shifted window.
    /// </summary>
    internal static string Instant(DateTimeOffset value)
        => value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
