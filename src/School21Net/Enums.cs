namespace School21Net;

/// <summary>Participant account status (<c>ParticipantV1DTO.status</c>).</summary>
public enum ParticipantStatus
{
    /// <summary>ACTIVE.</summary>
    Active,
    /// <summary>TEMPORARY_BLOCKING.</summary>
    TemporaryBlocking,
    /// <summary>EXPELLED.</summary>
    Expelled,
    /// <summary>BLOCKED.</summary>
    Blocked,
    /// <summary>FROZEN.</summary>
    Frozen,
    /// <summary>STUDY_COMPLETED.</summary>
    StudyCompleted
}

/// <summary>Project execution type (<c>ParticipantProjectV1DTO.type</c>).</summary>
public enum ParticipantProjectType
{
    /// <summary>INDIVIDUAL.</summary>
    Individual,
    /// <summary>GROUP.</summary>
    Group,
    /// <summary>EXAM.</summary>
    Exam,
    /// <summary>EXAM_TEST.</summary>
    ExamTest,
    /// <summary>INTERNSHIP.</summary>
    Internship
}

/// <summary>
/// A participant's status on a project (<c>ParticipantProjectV1DTO.status</c>). For matching:
/// <see cref="Accepted"/> = finished (can review), <see cref="InReviews"/> = submitted and awaiting a reviewer.
/// </summary>
public enum ParticipantProjectStatus
{
    /// <summary>ASSIGNED — on the roadmap, not started.</summary>
    Assigned,
    /// <summary>REGISTERED — registered for the project.</summary>
    Registered,
    /// <summary>IN_PROGRESS — being worked on.</summary>
    InProgress,
    /// <summary>IN_REVIEWS — submitted, awaiting peer review.</summary>
    InReviews,
    /// <summary>ACCEPTED — completed and passed.</summary>
    Accepted,
    /// <summary>FAILED — completed and failed.</summary>
    Failed
}

/// <summary>
/// The event kinds the <c>type</c> filter accepts on <c>GET /v1/events</c>.
/// <para>
/// Deliberately not the same vocabulary as <see cref="SchoolEvent.Type"/>, which is free text the
/// campus writes ("Клуб", "Митап"). Filtering and reading use different words; do not map one onto
/// the other.
/// </para>
/// </summary>
public enum EventType
{
    /// <summary>ACTIVITY.</summary>
    Activity,
    /// <summary>EXAM.</summary>
    Exam,
    /// <summary>TEST.</summary>
    Test
}

/// <summary>Status of a course on a participant's roadmap.</summary>
public enum ParticipantCourseStatus
{
    /// <summary>ASSIGNED.</summary>
    Assigned,
    /// <summary>REGISTERED.</summary>
    Registered,
    /// <summary>IN_PROGRESS.</summary>
    InProgress,
    /// <summary>ACCEPTED.</summary>
    Accepted,
    /// <summary>FAILED.</summary>
    Failed
}

/// <summary>Which points currency a sale applies to.</summary>
public enum SaleType
{
    /// <summary>PRP — peer review points.</summary>
    Prp,
    /// <summary>CRP — code review points.</summary>
    Crp
}

/// <summary>Whether a sale is running.</summary>
public enum SaleStatus
{
    /// <summary>NON_ACTIVE.</summary>
    NonActive,
    /// <summary>ACTIVE.</summary>
    Active,
    /// <summary>PLANNED.</summary>
    Planned
}

/// <summary>What a curriculum graph node item stands for.</summary>
public enum GraphEntityType
{
    /// <summary>PROJECT.</summary>
    Project,
    /// <summary>COURSE.</summary>
    Course
}
