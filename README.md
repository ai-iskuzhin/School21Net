<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="https://raw.githubusercontent.com/ai-iskuzhin/School21Net/main/assets/logo.png" width="140" alt="School21Net logo" />
    </td>
    <td valign="middle">
      <h1>School21Net</h1>
      <p>Typed .NET client for the official <a href="https://platform.21-school.ru">School 21</a> public API — participants, projects (finished / in-reviews), campuses and coalitions.</p>
      <p>
        <a href="LICENSE"><img src="https://img.shields.io/github/license/ai-iskuzhin/School21Net?style=flat-square" alt="License" /></a>
        <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/targets-net8.0%20%7C%20net10.0-512BD4?logo=dotnet&amp;style=flat-square" alt="Targets" /></a>
        <a href="https://www.nuget.org/packages/School21Net"><img src="https://img.shields.io/nuget/v/School21Net?logo=nuget&amp;style=flat-square" alt="NuGet" /></a>
      </p>
    </td>
  </tr>
</table>

> Independent project, not affiliated with School 21 / Sber. Authenticates with your own participant credentials.

Typed .NET client for the **official School 21 public API** (`platform.21-school.ru/services/21-school/api`).

## Install

```bash
dotnet add package School21Net
```

Targets `net8.0` and `net10.0` (in-box `System.Text.Json`). **No third-party dependencies.**

- Integrator-owned auth: a stateless `School21AuthClient` issues/refreshes tokens (ROPC password + `refresh_token` grants); the API client reads the bearer from a pluggable `ISchool21AccessTokenProvider`
- Resources: **Participants**, **Projects**, **Campuses**, **Coalitions**
- Per-project participant lists filterable by status — `Accepted` (finished) / `InReviews` (awaiting review)
- Tolerant `SCREAMING_SNAKE` enum converters that raise on unknown values
- Layered exceptions: `School21ApiException` / `School21ProtocolException` / `School21TransportException` / `School21ValidationException`

## Quick start

```csharp
using School21Net;
using School21Net.Authentication;

var http = new HttpClient();

// 1) Authenticate on your side (the SDK never stores your credentials).
var auth = new School21AuthClient(http);
var token = await auth.AuthenticateAsync("your-login", "your-password", ct); // keep the password in secrets/env

// 2) Give the client a token provider. StaticAccessTokenProvider is fine for scripts;
//    for long-running apps implement ISchool21AccessTokenProvider so you can refresh (see below).
var client = new School21Client(http, new School21ClientOptions(), new StaticAccessTokenProvider(token.AccessToken));

// Who finished PM2_MetricsDriven (projectId 73465)?
var finishers = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);

// Who is waiting for a reviewer on it (reciprocal-match pool)?
var awaiting = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.InReviews);

// A participant's project statuses
var me = await client.Participants.GetAsync("elenipad");
var projects = await client.Participants.GetProjectsAsync("elenipad", ParticipantProjectStatus.InReviews);
```

## Auth is owned by the integrator

The SDK does **not** store credentials or refresh tokens for you. `School21AuthClient` is a thin, stateless
wrapper over the token endpoint:

```csharp
var token   = await auth.AuthenticateAsync(login, password, ct); // password grant
var renewed = await auth.RefreshAsync(token.RefreshToken!, ct);   // refresh_token grant
```

`School21Client` asks an `ISchool21AccessTokenProvider` for a bearer before each request. You own the token
lifecycle — cache the current token, refresh before `token.AccessTokenExpiresAtUtc`, and re-authenticate once
the refresh token is gone (tokens carry a **~10-hour session ceiling**). A minimal refresh-aware provider:

```csharp
public sealed class School21TokenProvider(School21AuthClient auth, string login, string password)
    : ISchool21AccessTokenProvider
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private School21Token? _token;

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (_token is { } t && DateTimeOffset.UtcNow < t.AccessTokenExpiresAtUtc.AddMinutes(-1))
            return t.AccessToken;

        await _gate.WaitAsync(ct);
        try
        {
            if (_token is { } cur && DateTimeOffset.UtcNow < cur.AccessTokenExpiresAtUtc.AddMinutes(-1))
                return cur.AccessToken;

            _token = _token?.RefreshToken is { } rt && DateTimeOffset.UtcNow < _token.RefreshTokenExpiresAtUtc
                ? await SafeRefreshAsync(rt, ct)
                : await auth.AuthenticateAsync(login, password, ct);
            return _token.AccessToken;
        }
        finally { _gate.Release(); }
    }

    private async Task<School21Token> SafeRefreshAsync(string refreshToken, CancellationToken ct)
    {
        try { return await auth.RefreshAsync(refreshToken, ct); }
        catch (School21ApiException) { return await auth.AuthenticateAsync(login, password, ct); }
    }
}
```

No built-in DI helpers ship with the SDK — register `School21AuthClient`, your provider and `School21Client`
with your own container (e.g. `AddHttpClient<School21Client>()`), so nothing is hidden from you.

## Coverage

Implemented: participant basic info / projects / single project / coalition / points / feedback;
project participant lists (status/campus filters); campuses list / participants / coalitions;
coalition participants. Not yet wrapped (easy to add on the same pattern): graph, events, courses,
badges, skills, experience-history, logtime, workstation, clusters, sales.

## Build

```bash
dotnet build School21Net.slnx
dotnet test School21Net.slnx
```
