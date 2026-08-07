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

    /// <summary>Badges a participant has earned (<c>GET /v1/participants/{login}/badges</c>).</summary>
    public async Task<IReadOnlyList<ParticipantBadge>> GetBadgesAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        var envelope = await _client.GetAsync<ParticipantBadgesEnvelope>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/badges", cancellationToken)
            .ConfigureAwait(false);
        return envelope.Badges ?? [];
    }

    /// <summary>A participant's skills and their points (<c>GET /v1/participants/{login}/skills</c>).</summary>
    public async Task<IReadOnlyList<ParticipantSkill>> GetSkillsAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        var envelope = await _client.GetAsync<ParticipantSkillsEnvelope>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/skills", cancellationToken)
            .ConfigureAwait(false);
        return envelope.Skills ?? [];
    }

    /// <summary>Where a participant is sitting (<c>GET /v1/participants/{login}/workstation</c>).</summary>
    public Task<ParticipantWorkstation> GetWorkstationAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantWorkstation>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/workstation", cancellationToken);
    }

    /// <summary>
    /// Average hours a participant spends on campus per week
    /// (<c>GET /v1/participants/{login}/logtime</c>).
    /// <para>
    /// A bare number rather than an object, which is why this returns <see cref="double"/> and not a
    /// model — wrapping one figure in a record would invent a shape the API does not have.
    /// </para>
    /// </summary>
    /// <param name="login">Participant login.</param>
    /// <param name="date">Which month to report on, or null for the current one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<double> GetLogtimeAsync(
        string login,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        var path = $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/logtime";

        if (date is { } wanted)
        {
            path += School21Client.BuildQueryString(
                [new KeyValuePair<string, string>("date", wanted.ToString("yyyy-MM-dd"))]);
        }

        return _client.GetAsync<double>(path, cancellationToken);
    }

    /// <summary>XP accruals over time (<c>GET /v1/participants/{login}/experience-history</c>), paged.</summary>
    public Task<IReadOnlyList<ParticipantXpEntry>> GetExperienceHistoryAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetPagedAsync<ParticipantXpHistoryEnvelope, ParticipantXpEntry>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/experience-history",
            envelope => envelope.ExpHistory,
            query: null,
            cancellationToken);
    }

    /// <summary>Courses on a participant's roadmap (<c>GET /v1/participants/{login}/courses</c>), paged.</summary>
    /// <param name="login">Participant login.</param>
    /// <param name="status">Narrow to one status, or null for all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<IReadOnlyList<ParticipantCourse>> GetCoursesAsync(
        string login,
        ParticipantCourseStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        var query = status is { } wanted
            ? new List<KeyValuePair<string, string>> {new("status", ScreamingSnakeEnumConverter<ParticipantCourseStatus>.ToWire(wanted))}
            : null;

        return _client.GetPagedAsync<ParticipantCoursesEnvelope, ParticipantCourse>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/courses",
            envelope => envelope.Courses,
            query,
            cancellationToken);
    }

    /// <summary>One course on a participant's roadmap (<c>GET /v1/participants/{login}/courses/{courseId}</c>).</summary>
    public Task<ParticipantCourse> GetCourseAsync(
        string login,
        long courseId,
        CancellationToken cancellationToken = default)
    {
        School21WireParsing.RequireNonEmpty(login, nameof(login));
        return _client.GetAsync<ParticipantCourse>(
            $"/v1/participants/{School21WireParsing.EscapeSegment(login)}/courses/{courseId}", cancellationToken);
    }
}
