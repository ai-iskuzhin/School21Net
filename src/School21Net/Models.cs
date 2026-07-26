using System.Text.Json.Serialization;

namespace School21Net;

// Public models mirror the OpenAPI *V1DTO schemas. The wire is camelCase, matched by the Web naming policy.

/// <summary>Basic participant info (<c>GET /v1/participants/{login}</c>).</summary>
public sealed record Participant
{
    /// <summary>Participant login.</summary>
    public string Login { get; init; } = string.Empty;
    /// <summary>Current wave / class name (e.g. "26_04_UFA").</summary>
    public string? ClassName { get; init; }
    /// <summary>Education form / parallel (e.g. "Core program").</summary>
    public string? ParallelName { get; init; }
    /// <summary>Experience points.</summary>
    public int ExpValue { get; init; }
    /// <summary>Level.</summary>
    public int Level { get; init; }
    /// <summary>XP remaining to the next level.</summary>
    public int ExpToNextLevel { get; init; }
    /// <summary>Home campus.</summary>
    public ParticipantCampus? Campus { get; init; }
    /// <summary>Account status.</summary>
    public ParticipantStatus? Status { get; init; }
}

/// <summary>Campus reference embedded in a participant.</summary>
public sealed record ParticipantCampus
{
    /// <summary>Campus id (UUID).</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Short campus name (e.g. "21 Ufa").</summary>
    public string? ShortName { get; init; }
}

/// <summary>One project on a participant's roadmap (<c>ParticipantProjectV1DTO</c>).</summary>
public sealed record ParticipantProject
{
    /// <summary>Project id.</summary>
    public long Id { get; init; }
    /// <summary>Project title (e.g. "PM2_MetricsDriven").</summary>
    public string? Title { get; init; }
    /// <summary>Execution type.</summary>
    public ParticipantProjectType? Type { get; init; }
    /// <summary>Status — <see cref="ParticipantProjectStatus.Accepted"/> = finished, <see cref="ParticipantProjectStatus.InReviews"/> = awaiting review.</summary>
    public ParticipantProjectStatus? Status { get; init; }
    /// <summary>Final percentage, when completed.</summary>
    public int? FinalPercentage { get; init; }
    /// <summary>Completion timestamp (ISO 8601 string), when completed.</summary>
    public string? CompletionDateTime { get; init; }
    /// <summary>Team members, for group projects.</summary>
    public IReadOnlyList<TeamMember>? TeamMembers { get; init; }
    /// <summary>Owning course id, if any.</summary>
    public long? CourseId { get; init; }
}

/// <summary>A team member on a group project.</summary>
public sealed record TeamMember
{
    /// <summary>Member login.</summary>
    public string Login { get; init; } = string.Empty;
    /// <summary>Whether this member is the team lead.</summary>
    public bool IsTeamlead { get; init; }
}

/// <summary>A participant's coalition membership (<c>GET /v1/participants/{login}/coalition</c>).</summary>
public sealed record ParticipantCoalition
{
    /// <summary>Coalition id.</summary>
    public long CoalitionId { get; init; }
    /// <summary>Coalition name.</summary>
    public string? Name { get; init; }
    /// <summary>Participant's rank within the coalition.</summary>
    public int Rank { get; init; }
}

/// <summary>A participant's points (<c>GET /v1/participants/{login}/points</c>).</summary>
public sealed record ParticipantPoints
{
    /// <summary>Peer-review points.</summary>
    public int PeerReviewPoints { get; init; }
    /// <summary>Code-review points.</summary>
    public int CodeReviewPoints { get; init; }
    /// <summary>Coins balance.</summary>
    public int Coins { get; init; }
}

/// <summary>Average feedback a participant receives as a verifier (<c>GET /v1/participants/{login}/feedback</c>).</summary>
public sealed record ParticipantFeedback
{
    /// <summary>Average punctuality score.</summary>
    public double AverageVerifierPunctuality { get; init; }
    /// <summary>Average interest score.</summary>
    public double AverageVerifierInterest { get; init; }
    /// <summary>Average thoroughness score.</summary>
    public double AverageVerifierThoroughness { get; init; }
    /// <summary>Average friendliness score.</summary>
    public double AverageVerifierFriendliness { get; init; }
}

/// <summary>A campus (<c>GET /v1/campuses</c>).</summary>
public sealed record Campus
{
    /// <summary>Campus id (UUID).</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Short name (e.g. "21 Ufa").</summary>
    public string? ShortName { get; init; }
    /// <summary>Full name.</summary>
    public string? FullName { get; init; }
}

/// <summary>A coalition (<c>GET /v1/campuses/{campusId}/coalitions</c>).</summary>
public sealed record Coalition
{
    /// <summary>Coalition id.</summary>
    public long CoalitionId { get; init; }
    /// <summary>Coalition name.</summary>
    public string? Name { get; init; }
}

// ---- internal list envelopes ----

internal sealed class ParticipantProjectsEnvelope
{
    public List<ParticipantProject>? Projects { get; set; }
}

internal sealed class ParticipantLoginsEnvelope
{
    public List<string>? Participants { get; set; }
}

internal sealed class CampusesEnvelope
{
    public List<Campus>? Campuses { get; set; }
}

internal sealed class CoalitionsEnvelope
{
    public List<Coalition>? Coalitions { get; set; }
}

internal sealed record TokenResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
}
