namespace School21Net.Resources;

/// <summary>Project-scoped endpoints of the School 21 public API.</summary>
public sealed class ProjectsResource
{
    private readonly School21Client _client;

    internal ProjectsResource(School21Client client) => _client = client;

    /// <summary>
    /// All participant logins on a project (<c>GET /v1/projects/{projectId}/participants</c>), paged.
    /// Filter by <paramref name="status"/> — <see cref="ParticipantProjectStatus.Accepted"/> for finishers,
    /// <see cref="ParticipantProjectStatus.InReviews"/> for those awaiting review — and by <paramref name="campusId"/>.
    /// </summary>
    public Task<IReadOnlyList<string>> GetParticipantsAsync(
        long projectId,
        ParticipantProjectStatus? status = null,
        string? campusId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<KeyValuePair<string, string>>();
        if (status is { } value)
        {
            query.Add(new("status", ScreamingSnakeEnumConverter<ParticipantProjectStatus>.ToWire(value)));
        }

        if (!string.IsNullOrWhiteSpace(campusId))
        {
            query.Add(new("campusId", campusId!));
        }

        return _client.GetPagedAsync<ParticipantLoginsEnvelope, string>(
            $"/v1/projects/{projectId}/participants",
            envelope => envelope.Participants,
            query.Count > 0 ? query : null,
            cancellationToken);
    }
}
