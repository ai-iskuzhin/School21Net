namespace School21Net.Resources;

/// <summary>The curriculum graph (<c>GET /v1/graph</c>).</summary>
public sealed class GraphResource
{
    private readonly School21Client _client;

    internal GraphResource(School21Client client) => _client = client;

    /// <summary>
    /// The whole graph in one call: nodes holding projects and courses, and the edges between them.
    /// </summary>
    public Task<CurriculumGraph> GetAsync(CancellationToken cancellationToken = default)
        => _client.GetAsync<CurriculumGraph>("/v1/graph", cancellationToken);
}
