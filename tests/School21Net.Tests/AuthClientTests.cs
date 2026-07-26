using System.Net;
using School21Net.Authentication;

namespace School21Net.Tests;

public sealed class AuthClientTests
{
    private static School21AuthClient CreateAuth(StubHttpMessageHandler handler)
        => new(
            new HttpClient(handler),
            new School21AuthOptions { TokenEndpoint = "https://auth.example.test/token", ClientId = "s21-open-api" });

    [Fact]
    public async Task Authenticate_posts_password_grant_and_maps_token()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK,
            "{\"access_token\":\"at\",\"refresh_token\":\"rt\",\"expires_in\":3600,\"refresh_expires_in\":36000,\"token_type\":\"Bearer\"}"));
        var auth = CreateAuth(handler);

        var token = await auth.AuthenticateAsync("login", "secret", CancellationToken.None);

        Assert.Equal("at", token.AccessToken);
        Assert.Equal("rt", token.RefreshToken);
        Assert.Equal(3600, token.ExpiresIn);
        Assert.Equal(36000, token.RefreshExpiresIn);
        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(token.ObtainedAtUtc.AddSeconds(3600), token.AccessTokenExpiresAtUtc);
        Assert.Equal(token.ObtainedAtUtc.AddSeconds(36000), token.RefreshTokenExpiresAtUtc);

        var body = Assert.Single(handler.RequestBodies)!;
        Assert.Contains("grant_type=password", body);
        Assert.Contains("username=login", body);
        Assert.Contains("password=secret", body);
        Assert.Contains("client_id=s21-open-api", body);
    }

    [Fact]
    public async Task Refresh_posts_refresh_token_grant()
    {
        var handler = new StubHttpMessageHandler(_ => (HttpStatusCode.OK,
            "{\"access_token\":\"at2\",\"refresh_token\":\"rt2\",\"expires_in\":3600}"));
        var auth = CreateAuth(handler);

        var token = await auth.RefreshAsync("old-refresh", CancellationToken.None);

        Assert.Equal("at2", token.AccessToken);
        Assert.Equal("rt2", token.RefreshToken);
        Assert.Null(token.RefreshTokenExpiresAtUtc); // refresh_expires_in absent -> unknown

        var body = Assert.Single(handler.RequestBodies)!;
        Assert.Contains("grant_type=refresh_token", body);
        Assert.Contains("refresh_token=old-refresh", body);
        Assert.DoesNotContain("password", body);
    }

    [Fact]
    public async Task Authenticate_failure_throws_api_exception()
    {
        var handler = new StubHttpMessageHandler(_ =>
            (HttpStatusCode.Unauthorized, "{\"error\":\"invalid_grant\",\"error_description\":\"bad creds\"}"));
        var auth = CreateAuth(handler);

        var ex = await Assert.ThrowsAsync<School21ApiException>(
            () => auth.AuthenticateAsync("login", "wrong", CancellationToken.None));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("invalid_grant", ex.ErrorCode);
    }

    [Fact]
    public async Task Empty_username_throws_validation()
        => await Assert.ThrowsAsync<School21ValidationException>(
            () => CreateAuth(new StubHttpMessageHandler(_ => (HttpStatusCode.OK, "{}")))
                .AuthenticateAsync(" ", "x", CancellationToken.None));
}
