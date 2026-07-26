namespace School21Net.Resources;

/// <summary>Participant-scoped endpoints of the School 21 public API.</summary>
public sealed class ParticipantsResource
{
    private readonly School21Client _client;

    internal ParticipantsResource(School21Client client) => _client = client;

    /// <summary>Basic participant info (<c>GET /v1/participants/{login}</c>).</summary>
    public Task<Participant> GetAsync(string login, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<Participant>($"/v1/participants/{School21WireParsing.EscapeSegment(login)}", cancellationToken);
    }

    /// <summary>All of a participant's projects, optionally filtered by <paramref name="status"/> (paged).</summary>
    public Task<IReadOnlyList<ParticipantProject>> GetProjectsAsync(
        string login,
        ParticipantProjectStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        var query = status is { } value
            ? new[] { new KeyValuePair<string, string>("status", ScreamingSnakeEnumConverter<ParticipantProjectStatus>.ToWire(value)) }
            : null;
        return _client.GetPagedAsync<ParticipantProjectsEnvelope, ParticipantProject>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/projects",
            envelope => envelope.Projects,
            query,
            cancellationToken);
    }

    /// <summary>A single project on a participant's roadmap (<c>GET /v1/participants/{login}/projects/{projectId}</c>).</summary>
    public Task<ParticipantProject> GetProjectAsync(string login, long projectId, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantProject>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/projects/{projectId}", cancellationToken);
    }

    /// <summary>A participant's coalition membership (<c>GET /v1/participants/{login}/coalition</c>).</summary>
    public Task<ParticipantCoalition> GetCoalitionAsync(string login, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantCoalition>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/coalition", cancellationToken);
    }

    /// <summary>A participant's points (<c>GET /v1/participants/{login}/points</c>).</summary>
    public Task<ParticipantPoints> GetPointsAsync(string login, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantPoints>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/points", cancellationToken);
    }

    /// <summary>A participant's average verifier feedback (<c>GET /v1/participants/{login}/feedback</c>).</summary>
    public Task<ParticipantFeedback> GetFeedbackAsync(string login, CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantFeedback>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/feedback", cancellationToken);
    }
}
