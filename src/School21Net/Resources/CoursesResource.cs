namespace School21Net.Resources;

/// <summary>Curriculum course endpoints of the School 21 public API.</summary>
public sealed class CoursesResource
{
    private readonly School21Client _client;

    internal CoursesResource(School21Client client) => _client = client;

    /// <summary>One course (<c>GET /v1/courses/{courseId}</c>).</summary>
    public Task<Course> GetAsync(long courseId, CancellationToken cancellationToken = default)
        => _client.GetAsync<Course>($"/v1/courses/{courseId}", cancellationToken);
}
