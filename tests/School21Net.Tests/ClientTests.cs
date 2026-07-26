using System.Net;

namespace School21Net.Tests;

public sealed class ClientTests
{
    private static School21ClientOptions Options() => new()
    {
        BaseUrl = "https://example.test/api",
        TokenEndpoint = "https://auth.example.test/token",
        Username = "login",
        Password = "secret"
    };

    private static School21Client CreateClient(StubHttpMessageHandler handler)
        => new(new HttpClient(handler), Options());

    [Fact]
    public async Task GetParticipants_authenticates_then_sends_status_query()
    {
        var handler = new StubHttpMessageHandler(_ =>
            (HttpStatusCode.OK, "{\"participants\":[\"a\",\"b\",\"c\"]}"));
        var client = CreateClient(handler);

        var logins = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);

        Assert.Equal(["a", "b", "c"], logins);

        // First request is the token exchange, second is the participants call.
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        var apiCall = handler.Requests[1];
        Assert.Contains("/v1/projects/73465/participants", apiCall.RequestUri!.AbsolutePath);
        Assert.Contains("status=ACCEPTED", apiCall.RequestUri!.Query);
        Assert.Contains("limit=1000", apiCall.RequestUri!.Query);
    }

    [Fact]
    public async Task Reuses_cached_token_across_calls()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, "{\"participants\":[]}"));
        var client = CreateClient(handler);

        await client.Projects.GetParticipantsAsync(1);
        await client.Coalitions.GetParticipantsAsync(2);

        Assert.Single(handler.Requests, r =>
            r.Method == HttpMethod.Post && r.RequestUri!.AbsolutePath.Contains("token"));
    }

    [Fact]
    public async Task Non_success_throws_api_exception()
    {
        var handler = new StubHttpMessageHandler(_ =>
            (HttpStatusCode.Forbidden, "{\"error\":\"forbidden\",\"error_description\":\"nope\"}"));
        var client = CreateClient(handler);

        var ex = await Assert.ThrowsAsync<School21ApiException>(() => client.Participants.GetAsync("elenipad"));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.True(ex.IsAuthError);
        Assert.Equal("forbidden", ex.ErrorCode);
    }

    [Fact]
    public async Task Empty_login_throws_validation()
        => await Assert.ThrowsAsync<School21ValidationException>(
            () => CreateClient(new StubHttpMessageHandler(_ => (HttpStatusCode.OK, "{}"))).Participants.GetAsync(" "));
}
