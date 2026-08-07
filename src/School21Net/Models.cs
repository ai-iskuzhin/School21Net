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

// ---------------------------------------------------------------------------------------------
// Events, clusters, courses, skills, badges, sales and the curriculum graph.
// ---------------------------------------------------------------------------------------------

/// <summary>One campus event (<c>EventV1DTO</c>).</summary>
/// <remarks>
/// The feed is scoped to the campus of the account the token belongs to. There is no campus
/// parameter, so a client sees one campus and cannot ask for another.
/// </remarks>
public sealed record SchoolEvent
{
    /// <summary>Event id.</summary>
    public long Id { get; init; }

    /// <summary>
    /// Free text as the campus writes it — "Клуб", "Митап", "Мероприятие". Not the
    /// <see cref="EventType"/> enum used to filter, which is a different vocabulary.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>Event title.</summary>
    public string? Name { get; init; }

    /// <summary>Free-text description.</summary>
    public string? Description { get; init; }

    /// <summary>Where it happens, as written — "Кампус 2й этаж".</summary>
    public string? Location { get; init; }

    /// <summary>Start, in UTC.</summary>
    public DateTimeOffset StartDateTime { get; init; }

    /// <summary>End, in UTC.</summary>
    public DateTimeOffset EndDateTime { get; init; }

    /// <summary>Organiser logins.</summary>
    public IReadOnlyList<string> Organizers { get; init; } = [];

    /// <summary>How many places exist.</summary>
    public int Capacity { get; init; }

    /// <summary>How many are taken. Equal to <see cref="Capacity"/> means full.</summary>
    public int RegisterCount { get; init; }
}

/// <summary>A cluster of workplaces in a campus (<c>ClusterV1DTO</c>).</summary>
public sealed record Cluster
{
    /// <summary>Cluster id.</summary>
    public long Id { get; init; }
    /// <summary>Cluster name.</summary>
    public string? Name { get; init; }
    /// <summary>Which floor it is on.</summary>
    public int Floor { get; init; }
    /// <summary>Total workplaces.</summary>
    public int Capacity { get; init; }
    /// <summary>Workplaces free right now.</summary>
    public int AvailableCapacity { get; init; }
}

/// <summary>One workplace on a cluster map (<c>WorkplaceV1DTO</c>).</summary>
public sealed record Workplace
{
    /// <summary>Row label.</summary>
    public string? Row { get; init; }
    /// <summary>Seat number within the row.</summary>
    public int Number { get; init; }
    /// <summary>Who is sitting there, or null when it is free.</summary>
    public string? Login { get; init; }
}

/// <summary>Where a participant is sitting (<c>ParticipantWorkstationV1DTO</c>).</summary>
public sealed record ParticipantWorkstation
{
    /// <summary>Cluster id.</summary>
    public long ClusterId { get; init; }
    /// <summary>Cluster name.</summary>
    public string? ClusterName { get; init; }
    /// <summary>Row label.</summary>
    public string? Row { get; init; }
    /// <summary>Seat number.</summary>
    public int Number { get; init; }
}

/// <summary>A badge a participant has earned (<c>ParticipantBadgeV1DTO</c>).</summary>
public sealed record ParticipantBadge
{
    /// <summary>Badge name.</summary>
    public string? Name { get; init; }
    /// <summary>Icon URL.</summary>
    public string? IconUrl { get; init; }
    /// <summary>When it was awarded.</summary>
    public DateTimeOffset ReceiptDateTime { get; init; }
}

/// <summary>A skill and its points (<c>ParticipantSkillV1DTO</c>).</summary>
public sealed record ParticipantSkill
{
    /// <summary>Skill name.</summary>
    public string? Name { get; init; }
    /// <summary>Points accumulated in it.</summary>
    public int Points { get; init; }
}

/// <summary>One XP accrual (<c>ParticipantXpHistoryItemV1DTO</c>).</summary>
public sealed record ParticipantXpEntry
{
    /// <summary>When the XP was credited.</summary>
    public DateTimeOffset AccrualDateTime { get; init; }
    /// <summary>How much.</summary>
    public long ExpValue { get; init; }
}

/// <summary>A course on a participant's roadmap (<c>ParticipantCourseV1DTO</c>).</summary>
public sealed record ParticipantCourse
{
    /// <summary>Course id.</summary>
    public long Id { get; init; }
    /// <summary>Course title.</summary>
    public string? Title { get; init; }
    /// <summary>Status.</summary>
    public ParticipantCourseStatus? Status { get; init; }
    /// <summary>Final mark, once there is one.</summary>
    public int? FinalPercentage { get; init; }
    /// <summary>When it was completed.</summary>
    public DateTimeOffset? CompletionDateTime { get; init; }
}

/// <summary>A course in the curriculum (<c>CourseV1DTO</c>).</summary>
public sealed record Course
{
    /// <summary>Course id.</summary>
    public long CourseId { get; init; }
    /// <summary>Title.</summary>
    public string? Title { get; init; }
    /// <summary>Description.</summary>
    public string? Description { get; init; }
    /// <summary>Expected duration in hours.</summary>
    public int DurationHours { get; init; }
    /// <summary>XP awarded.</summary>
    public int Xp { get; init; }
}

/// <summary>A project in the curriculum (<c>ProjectV1DTO</c>).</summary>
public sealed record Project
{
    /// <summary>Project id.</summary>
    public long ProjectId { get; init; }
    /// <summary>The course it belongs to.</summary>
    public long CourseId { get; init; }
    /// <summary>Title.</summary>
    public string? Title { get; init; }
    /// <summary>Description.</summary>
    public string? Description { get; init; }
    /// <summary>Execution type — group projects need a team before they can start.</summary>
    public ParticipantProjectType? Type { get; init; }
    /// <summary>Expected duration in hours.</summary>
    public int DurationHours { get; init; }
    /// <summary>XP awarded.</summary>
    public int Xp { get; init; }
}

/// <summary>A points sale window (<c>SaleV1DTO</c>).</summary>
public sealed record Sale
{
    /// <summary>Which currency the sale applies to.</summary>
    public SaleType? Type { get; init; }
    /// <summary>Whether it is running, finished or upcoming.</summary>
    public SaleStatus? Status { get; init; }
    /// <summary>When it starts.</summary>
    public DateTimeOffset StartDateTime { get; init; }
    /// <summary>How far through it is.</summary>
    public int ProgressPercentage { get; init; }
}

/// <summary>The curriculum graph (<c>GraphV1DTO</c>).</summary>
public sealed record CurriculumGraph
{
    /// <summary>Nodes, each holding one or more projects or courses.</summary>
    public IReadOnlyList<GraphNode> Nodes { get; init; } = [];
    /// <summary>Edges between nodes.</summary>
    public IReadOnlyList<GraphEdge> Edges { get; init; } = [];
}

/// <summary>One node of the curriculum graph (<c>GraphNodeV1DTO</c>).</summary>
public sealed record GraphNode
{
    /// <summary>Node id, referenced by edges.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Display label.</summary>
    public string? Label { get; init; }
    /// <summary>What the node contains.</summary>
    public IReadOnlyList<GraphNodeItem> Items { get; init; } = [];
}

/// <summary>A project or course sitting in a graph node (<c>GraphNodeItemV1DTO</c>).</summary>
public sealed record GraphNodeItem
{
    /// <summary>Item id within the graph.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Project or course code.</summary>
    public string? Code { get; init; }
    /// <summary>The id of the project or course this stands for.</summary>
    public long EntityId { get; init; }
    /// <summary>Whether <see cref="EntityId"/> is a project or a course.</summary>
    public GraphEntityType? EntityType { get; init; }
    /// <summary>Connection handles edges attach to.</summary>
    public IReadOnlyList<string> Handles { get; init; } = [];
}

/// <summary>An edge of the curriculum graph (<c>GraphEdgeV1DTO</c>).</summary>
public sealed record GraphEdge
{
    /// <summary>Edge id.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Node the edge leaves.</summary>
    public string? Source { get; init; }
    /// <summary>Handle on the source it leaves from.</summary>
    public string? SourceHandle { get; init; }
    /// <summary>Node the edge enters.</summary>
    public string? Target { get; init; }
    /// <summary>Handle on the target it enters.</summary>
    public string? TargetHandle { get; init; }
}

internal sealed record EventsEnvelope
{
    public IReadOnlyList<SchoolEvent>? Events { get; init; }
}

internal sealed record ClustersEnvelope
{
    public IReadOnlyList<Cluster>? Clusters { get; init; }
}

internal sealed record ClusterMapEnvelope
{
    public IReadOnlyList<Workplace>? ClusterMap { get; init; }
}

internal sealed record ParticipantBadgesEnvelope
{
    public IReadOnlyList<ParticipantBadge>? Badges { get; init; }
}

internal sealed record ParticipantSkillsEnvelope
{
    public IReadOnlyList<ParticipantSkill>? Skills { get; init; }
}

internal sealed record ParticipantXpHistoryEnvelope
{
    public IReadOnlyList<ParticipantXpEntry>? ExpHistory { get; init; }
}

internal sealed record ParticipantCoursesEnvelope
{
    public IReadOnlyList<ParticipantCourse>? Courses { get; init; }
}

internal sealed record SalesEnvelope
{
    public IReadOnlyList<Sale>? Sales { get; init; }
}
