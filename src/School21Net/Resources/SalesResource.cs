namespace School21Net.Resources;

/// <summary>Points sales (<c>GET /v1/sales</c>).</summary>
public sealed class SalesResource
{
    private readonly School21Client _client;

    internal SalesResource(School21Client client) => _client = client;

    /// <summary>Current and planned sales on peer and code review points.</summary>
    public async Task<IReadOnlyList<Sale>> GetAsync(CancellationToken cancellationToken = default)
    {
        var envelope = await _client.GetAsync<SalesEnvelope>("/v1/sales", cancellationToken).ConfigureAwait(false);
        return envelope.Sales ?? [];
    }
}
