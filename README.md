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

Targets `net8.0` and `net10.0` (in-box `System.Text.Json`).

- ROPC password-grant auth (`client_id=s21-open-api`) with transparent token caching/refresh
- Resources: **Participants**, **Projects**, **Campuses**, **Coalitions**
- Per-project participant lists filterable by status — `Accepted` (finished) / `InReviews` (awaiting review)
- Tolerant `SCREAMING_SNAKE` enum converters that raise on unknown values
- Layered exceptions: `School21ApiException` / `School21ProtocolException` / `School21TransportException` / `School21ValidationException`

## Quick start

```csharp
using School21Net;

var client = new School21Client(new HttpClient(), new School21ClientOptions
{
    Username = "your-login",
    Password = "your-password", // keep in secrets/env
});

// Who finished PM2_MetricsDriven (projectId 73465)?
var finishers = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.Accepted);

// Who is waiting for a reviewer on it (reciprocal-match pool)?
var awaiting = await client.Projects.GetParticipantsAsync(73465, ParticipantProjectStatus.InReviews);

// A participant's project statuses
var me = await client.Participants.GetAsync("elenipad");
var projects = await client.Participants.GetProjectsAsync("elenipad", ParticipantProjectStatus.InReviews);
```

## Dependency injection

```csharp
services.AddSchool21Net(new School21ClientOptions
{
    Username = configuration["School21Net:Username"]!,
    Password = configuration["School21Net:Password"]!,
});
// then inject School21Client
```

## Auth notes

Tokens carry a **~10-hour session ceiling**: the client refreshes transparently within it and
re-authenticates with the credentials afterwards. Store credentials in secrets/env, never in source.

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
