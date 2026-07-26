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
