namespace School21Net.Resources;

/// <summary>Coalition-scoped endpoints of the School 21 public API.</summary>
public sealed class CoalitionsResource
{
    private readonly School21Client _client;

    internal CoalitionsResource(School21Client client) => _client = client;

    /// <summary>All participant logins in a coalition (<c>GET /v1/coalitions/{coalitionId}/participants</c>), paged.</summary>
    public Task<IReadOnlyList<string>> GetParticipantsAsync(long coalitionId, CancellationToken cancellationToken = default)
        => _client.GetPagedAsync<ParticipantLoginsEnvelope, string>(
            $"/v1/coalitions/{coalitionId}/participants",
            envelope => envelope.Participants,
            query: null,
            cancellationToken);
}
