namespace School21Net.Resources;

/// <summary>Campus-scoped endpoints of the School 21 public API.</summary>
public sealed class CampusesResource
{
    private readonly School21Client _client;

    internal CampusesResource(School21Client client) => _client = client;

    /// <summary>All campuses (<c>GET /v1/campuses</c>).</summary>
    public async Task<IReadOnlyList<Campus>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var envelope = await _client.GetAsync<CampusesEnvelope>("/v1/campuses", cancellationToken).ConfigureAwait(false);
        return envelope.Campuses ?? [];
    }

    /// <summary>All participant logins in a campus (<c>GET /v1/campuses/{campusId}/participants</c>), paged.</summary>
    public Task<IReadOnlyList<string>> GetParticipantsAsync(string campusId, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(campusId, nameof(campusId));
        return _client.GetPagedAsync<ParticipantLoginsEnvelope, string>(
            $"/v1/campuses/{School21WireParsing.EscapeSegment(campusId)}/participants",
            envelope => envelope.Participants,
            query: null,
            cancellationToken);
    }

    /// <summary>All coalitions in a campus (<c>GET /v1/campuses/{campusId}/coalitions</c>), paged.</summary>
    public Task<IReadOnlyList<Coalition>> GetCoalitionsAsync(string campusId, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(campusId, nameof(campusId));
        return _client.GetPagedAsync<CoalitionsEnvelope, Coalition>(
            $"/v1/campuses/{School21WireParsing.EscapeSegment(campusId)}/coalitions",
            envelope => envelope.Coalitions,
            query: null,
            cancellationToken);
    }
}
