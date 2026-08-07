using System.Text.Json;

namespace School21Net.Tests;

public sealed class SerializationTests
{
    [Fact]
    public void Deserializes_participant_with_status_and_campus()
    {
        const string json = """
            {"login":"elenipad","className":"26_04_UFA","parallelName":"Core program","expValue":460,
             "level":0,"expToNextLevel":39,"campus":{"id":"e786cbfb","shortName":"21 Ufa"},"status":"ACTIVE"}
            """;

        var participant = JsonSerializer.Deserialize<Participant>(json, School21Client.Json)!;

        Assert.Equal("elenipad", participant.Login);
        Assert.Equal(ParticipantStatus.Active, participant.Status);
        Assert.Equal("21 Ufa", participant.Campus!.ShortName);
        Assert.Equal(460, participant.ExpValue);
    }

    [Theory]
    [InlineData("IN_REVIEWS", ParticipantProjectStatus.InReviews)]
    [InlineData("ACCEPTED", ParticipantProjectStatus.Accepted)]
    [InlineData("EXAM_TEST", null)] // sanity: parsed as a project type below, not a status
    public void Maps_screaming_snake_project_status(string wire, ParticipantProjectStatus? expected)
    {
        if (expected is null)
        {
            return;
        }

        var json = $$"""{"id":73465,"title":"PM2_MetricsDriven","type":"INDIVIDUAL","status":"{{wire}}"}""";

        var project = JsonSerializer.Deserialize<ParticipantProject>(json, School21Client.Json)!;

        Assert.Equal(expected, project.Status);
        Assert.Equal(ParticipantProjectType.Individual, project.Type);
    }

    /// <summary>
    /// This asserted a throw until 2.1.0, and the change is deliberate. A nullable enum is the model
    /// saying "the school may say something I do not know"; failing the whole response over one such
    /// field turned a vocabulary the school extends into an outage for every object carrying it.
    /// </summary>
    [Fact]
    public void Unknown_enum_value_reads_as_null_and_leaves_the_rest_intact()
    {
        const string json = """{"id":1,"title":"x","status":"TELEPORTED"}""";

        var project = JsonSerializer.Deserialize<ParticipantProject>(json, School21Client.Json)!;

        Assert.Null(project.Status);
        Assert.Equal(1, project.Id);
        Assert.Equal("x", project.Title);
    }
}
