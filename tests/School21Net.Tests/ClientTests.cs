using System.Net;
using School21Net.Authentication;

namespace School21Net.Tests;

public sealed class ClientTests
{
    private static School21Client CreateClient(StubHttpMessageHandler handler, string accessToken = "test-token")
        => new(
            new HttpClient(handler),
            new School21ClientOptions { BaseUrl = "https://example.test/api" },
            new StaticAccessTokenProvider(accessToken));

    [Fact]
    public async Task GetParticipants_sends_bearer_and_status_query()
    {
        var handler = new StubHttpMessageHandler(_ =>
            (HttpStatusCode.OK, "{\"participants\":[\"a\",\"b\",\"c\"]}"));
        var client = CreateClient(handler, "abc123");

        var logins = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);

        Assert.Equal(["a", "b", "c"], logins);

        // No token exchange happens on the client — auth is external. The only call is the API request,
        // carrying the bearer supplied by the token provider.
        var apiCall = Assert.Single(handler.Requests);
        Assert.Equal("Bearer", apiCall.Headers.Authorization!.Scheme);
        Assert.Equal("abc123", apiCall.Headers.Authorization!.Parameter);
        Assert.Contains("/v1/projects/73465/participants", apiCall.RequestUri!.AbsolutePath);
        Assert.Contains("status=ACCEPTED", apiCall.RequestUri!.Query);
        Assert.Contains("limit=1000", apiCall.RequestUri!.Query);
    }

    [Fact]
    public async Task Uses_provider_token_on_every_call()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK, "{\"participants\":[]}"));
        var client = CreateClient(handler, "bearer-xyz");

        await client.Projects.GetParticipantsAsync(1);
        await client.Coalitions.GetParticipantsAsync(2);

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, r => Assert.Equal("bearer-xyz", r.Headers.Authorization!.Parameter));
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
