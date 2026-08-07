using System.Net;
using System.Reflection;
using System.Text.Json;
using School21Net.Authentication;

namespace School21Net.Tests;

/// <summary>How enums cross the wire, and what happens when the school says something new.</summary>
public sealed class EnumWireTests
{
    private static School21Client CreateClient(string body)
        => new(
            new HttpClient(new StubHttpMessageHandler(_ => (HttpStatusCode.OK, body))),
            new School21ClientOptions {BaseUrl = "https://example.test/api"},
            new StaticAccessTokenProvider("t"));

    /// <summary>
    /// Every public enum, found by reflection rather than listed. A list is what this replaces: the
    /// converters used to be registered one line each, and the line nobody wrote was discovered by
    /// the production API rather than by a test.
    /// </summary>
    public static TheoryData<Type> PublicEnums()
    {
        var data = new TheoryData<Type>();

        foreach (var type in typeof(School21Client).Assembly.GetExportedTypes().Where(t => t.IsEnum))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(PublicEnums))]
    public void Every_public_enum_round_trips_through_its_wire_form(Type enumType)
    {
        var options = School21Client.Json;

        foreach (var value in Enum.GetValues(enumType))
        {
            var json = JsonSerializer.Serialize(value, enumType, options);

            Assert.StartsWith("\"", json);
            Assert.Equal(json.ToUpperInvariant(), json);
            Assert.Equal(value, JsonSerializer.Deserialize(json, enumType, options));
        }
    }

    /// <summary>
    /// The failure this guards against is total, not partial. A status the school adds would
    /// otherwise throw for every participant carrying it — stopping profile sync for those members
    /// entirely, over one field nobody was reading.
    /// </summary>
    [Fact]
    public async Task An_unknown_value_reads_as_null_rather_than_failing_the_response()
    {
        var client = CreateClient(
            """{"login":"elenipad","level":7,"status":"SOMETHING_THE_SCHOOL_ADDED_LATER"}""");

        var participant = await client.Participants.GetAsync("elenipad");

        Assert.Null(participant.Status);

        // And the rest of the response survives, which is the entire point.
        Assert.Equal("elenipad", participant.Login);
        Assert.Equal(7, participant.Level);
    }

    [Fact]
    public async Task A_known_value_still_reads_as_itself()
    {
        var client = CreateClient("""{"login":"elenipad","status":"EXPELLED"}""");

        Assert.Equal(ParticipantStatus.Expelled, (await client.Participants.GetAsync("elenipad")).Status);
    }

    [Fact]
    public async Task An_unknown_value_nested_in_a_list_does_not_take_the_list_with_it()
    {
        var client = CreateClient(
            """{"sales":[{"type":"PRP","status":"ACTIVE"},{"type":"NEW_CURRENCY","status":"ACTIVE"}]}""");

        var sales = await client.Sales.GetAsync();

        Assert.Equal(2, sales.Count);
        Assert.Equal(SaleType.Prp, sales[0].Type);
        Assert.Null(sales[1].Type);
        Assert.Equal(SaleStatus.Active, sales[1].Status);
    }

    /// <summary>
    /// A non-nullable enum property has nowhere to put an unknown, so it must still throw. Nothing on
    /// the models is one today — this pins the converter's behaviour for the day something is.
    /// </summary>
    [Fact]
    public void A_non_nullable_enum_still_refuses_a_value_it_does_not_know()
    {
        var options = School21Client.Json;

        Assert.ThrowsAny<JsonException>(
            () => JsonSerializer.Deserialize<ParticipantStatus>("\"NOT_A_STATUS\"", options));
    }

    /// <summary>
    /// No model may declare a non-nullable enum, for the reason above: it cannot survive the school
    /// extending its vocabulary. Checked over the whole assembly so a new model cannot quietly
    /// reintroduce the hazard.
    /// </summary>
    [Fact]
    public void No_model_exposes_a_non_nullable_enum()
    {
        var offenders = typeof(School21Client).Assembly.GetExportedTypes()
            // Models only. An exception carrying an HttpStatusCode is not a wire model, and its enum
            // comes from the framework rather than from the school's vocabulary.
            .Where(type => type.IsClass
                && type.Namespace == "School21Net"
                && !typeof(Exception).IsAssignableFrom(type))
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(property => property.PropertyType.IsEnum)
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")
            .ToArray();

        Assert.Empty(offenders);
    }
}
