using System.Net;
using School21Net.Authentication;
using School21Net.Resources;

namespace School21Net.Tests;

/// <summary>The endpoints added in 2.1: events, clusters, courses, graph, sales and the rest.</summary>
public sealed class EventsAndCatalogueTests
{
    private static School21Client CreateClient(StubHttpMessageHandler handler)
        => new(
            new HttpClient(handler),
            new School21ClientOptions {BaseUrl = "https://example.test/api"},
            new StaticAccessTokenProvider("test-token"));

    private const string OneEvent = """
        {"events":[{"id":3453985,"type":"Клуб","name":"Встреча клуба","description":"Играем",
        "location":"Кампус 2й этаж","startDateTime":"2026-08-01T05:00:00Z",
        "endDateTime":"2026-08-01T08:45:00Z","organizers":["muncited"],"capacity":10,"registerCount":2}]}
        """;

    /// <summary>
    /// The detail that makes this endpoint work at all. It rejects <c>2026-08-01</c> and also
    /// <c>2026-08-01T00:00:00</c> with a bare 400 that names no field; only a zoned instant is
    /// accepted. Formatting it here is the whole reason a caller cannot get it wrong.
    /// </summary>
    [Fact]
    public async Task The_window_is_sent_as_a_zoned_utc_instant()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, OneEvent));
        var client = CreateClient(handler);

        await client.Events.GetAsync(
            new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 31, 23, 59, 59, TimeSpan.Zero));

        var query = Uri.UnescapeDataString(Assert.Single(handler.Requests).RequestUri!.Query);

        Assert.Contains("from=2026-08-01T00:00:00Z", query);
        Assert.Contains("to=2026-08-31T23:59:59Z", query);
    }

    /// <summary>
    /// A caller passing a local time means that instant, not those digits. Sending the digits would
    /// silently shift the window by the offset and quietly miss events at either end.
    /// </summary>
    [Fact]
    public void A_local_time_is_converted_rather_than_relabelled()
        => Assert.Equal(
            "2026-08-01T00:00:00Z",
            EventsResource.Instant(new DateTimeOffset(2026, 8, 1, 5, 0, 0, TimeSpan.FromHours(5))));

    [Fact]
    public async Task Events_are_parsed_whole()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, OneEvent));
        var client = CreateClient(handler);

        var single = Assert.Single(await client.Events.GetAsync(
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddDays(1)));

        Assert.Equal(3453985, single.Id);
        Assert.Equal("Клуб", single.Type);
        Assert.Equal("Встреча клуба", single.Name);
        Assert.Equal("Кампус 2й этаж", single.Location);
        Assert.Equal(new DateTimeOffset(2026, 8, 1, 5, 0, 0, TimeSpan.Zero), single.StartDateTime);
        Assert.Equal(["muncited"], single.Organizers);
        Assert.Equal(10, single.Capacity);
        Assert.Equal(2, single.RegisterCount);
    }

    [Fact]
    public async Task The_type_filter_travels_as_the_wire_spelling()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, """{"events":[]}"""));
        var client = CreateClient(handler);

        await client.Events.GetAsync(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, EventType.Exam);

        Assert.Contains("type=EXAM", Assert.Single(handler.Requests).RequestUri!.Query);
    }

    [Fact]
    public async Task A_backwards_window_is_refused_before_it_is_sent()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, """{"events":[]}"""));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.Events.GetAsync(
            DateTimeOffset.UnixEpoch.AddDays(1),
            DateTimeOffset.UnixEpoch));

        Assert.Empty(handler.Requests);
    }

    /// <summary>Logtime answers with a bare number, so the SDK does too rather than inventing a wrapper.</summary>
    [Fact]
    public async Task Logtime_is_a_number_and_takes_an_optional_date()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, "3.25"));
        var client = CreateClient(handler);

        Assert.Equal(3.25, await client.Participants.GetLogtimeAsync("elenipad", new DateOnly(2026, 8, 1)));
        Assert.Contains("date=2026-08-01", Assert.Single(handler.Requests).RequestUri!.Query);
    }

    [Fact]
    public async Task A_cluster_map_can_ask_for_only_the_taken_seats()
    {
        var handler = new StubHttpMessageHandler(_ =>
            (HttpStatusCode.OK, """{"clusterMap":[{"row":"A","number":3,"login":"elenipad"}]}"""));
        var client = CreateClient(handler);

        var seat = Assert.Single(await client.Clusters.GetMapAsync(12, occupied: true));

        Assert.Equal("A", seat.Row);
        Assert.Equal(3, seat.Number);
        Assert.Equal("elenipad", seat.Login);
        Assert.Contains("occupied=true", Assert.Single(handler.Requests).RequestUri!.Query);
    }

    [Fact]
    public async Task Clusters_courses_projects_and_sales_are_reachable()
    {
        var handler = new StubHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath switch
            {
                var p when p.EndsWith("/clusters") =>
                    (HttpStatusCode.OK, """{"clusters":[{"id":1,"name":"c1","floor":2,"capacity":30,"availableCapacity":4}]}"""),
                var p when p.Contains("/courses/") =>
                    (HttpStatusCode.OK, """{"courseId":7,"title":"C","durationHours":40,"xp":100}"""),
                var p when p.Contains("/projects/") =>
                    (HttpStatusCode.OK, """{"projectId":9,"courseId":7,"title":"P","type":"GROUP","durationHours":20,"xp":50}"""),
                _ => (HttpStatusCode.OK, """{"sales":[{"type":"PRP","status":"ACTIVE","startDateTime":"2026-08-01T00:00:00Z","progressPercentage":40}]}"""),
            });
        var client = CreateClient(handler);

        Assert.Equal(4, (await client.Campuses.GetClustersAsync("campus-1")).Single().AvailableCapacity);
        Assert.Equal(40, (await client.Courses.GetAsync(7)).DurationHours);

        // The one field this service actually gates on: a group project cannot start without a team.
        Assert.Equal(ParticipantProjectType.Group, (await client.Projects.GetAsync(9)).Type);

        var sale = (await client.Sales.GetAsync()).Single();
        Assert.Equal(SaleType.Prp, sale.Type);
        Assert.Equal(SaleStatus.Active, sale.Status);
    }

    [Fact]
    public async Task Badges_skills_and_experience_history_are_unwrapped()
    {
        var handler = new StubHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath switch
            {
                var p when p.EndsWith("/badges") =>
                    (HttpStatusCode.OK, """{"badges":[{"name":"b","iconUrl":"u","receiptDateTime":"2026-08-01T00:00:00Z"}]}"""),
                var p when p.EndsWith("/skills") =>
                    (HttpStatusCode.OK, """{"skills":[{"name":"s","points":12}]}"""),
                _ =>
                    (HttpStatusCode.OK, """{"expHistory":[{"accrualDateTime":"2026-08-01T00:00:00Z","expValue":250}]}"""),
            });
        var client = CreateClient(handler);

        Assert.Equal("b", (await client.Participants.GetBadgesAsync("elenipad")).Single().Name);
        Assert.Equal(12, (await client.Participants.GetSkillsAsync("elenipad")).Single().Points);
        Assert.Equal(250, (await client.Participants.GetExperienceHistoryAsync("elenipad")).Single().ExpValue);
    }
}
