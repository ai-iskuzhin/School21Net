namespace School21Net.Resources;

/// <summary>Cluster-scoped endpoints of the School 21 public API.</summary>
public sealed class ClustersResource
{
    private readonly School21Client _client;

    internal ClustersResource(School21Client client) => _client = client;

    /// <summary>Every workplace in a cluster (<c>GET /v1/clusters/{clusterId}/map</c>), paged.</summary>
    /// <param name="clusterId">Cluster id.</param>
    /// <param name="occupied">Only taken seats when true, only free ones when false, all when null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<IReadOnlyList<Workplace>> GetMapAsync(
        long clusterId,
        bool? occupied = null,
        CancellationToken cancellationToken = default)
    {
        var query = occupied is { } wanted
            ? new List<KeyValuePair<string, string>> {new("occupied", wanted ? "true" : "false")}
            : null;

        return _client.GetPagedAsync<ClusterMapEnvelope, Workplace>(
            $"/v1/clusters/{clusterId}/map",
            envelope => envelope.ClusterMap,
            query,
            cancellationToken);
    }
}
