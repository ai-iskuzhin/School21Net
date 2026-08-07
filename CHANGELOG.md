# Changelog

## 2.1.0 — 2026-08-07

Full coverage of the public API, and enums that survive the school extending its vocabulary.

### Added — every endpoint in the spec

Coverage goes from 11 of 25 paths to **25 of 25**, checked mechanically against
`docs/openapi/school21-openapi.json`, which is now kept in the repository.

- `client.Events` — campus events over a window, optionally filtered by kind.
- `client.Clusters` — cluster maps, optionally only occupied seats.
- `client.Courses` — a curriculum course.
- `client.Graph` — the curriculum graph, nodes and edges.
- `client.Sales` — PRP and CRP sale windows.
- `client.Projects.GetAsync` — a curriculum project, including whether it is a group project.
- `client.Campuses.GetClustersAsync` — clusters in a campus.
- `client.Participants` — `GetBadgesAsync`, `GetSkillsAsync`, `GetWorkstationAsync`,
  `GetLogtimeAsync`, `GetExperienceHistoryAsync`, `GetCoursesAsync`, `GetCourseAsync`.

### Changed — an unknown enum value no longer fails the response

**This is a behaviour change and the reason for it is worth reading.** A nullable enum property used
to throw when the API sent a value this version did not know. For a client library that means the
day 21School adds a participant status, *every* call whose response contains it fails — profile sync
stops for those members over one field the caller may not even read.

Unknown values on nullable properties now read as `null`. A non-nullable enum still throws, because
it has nowhere to put an unknown; a test asserts no model declares one.

If you were catching `JsonException` to detect new server values, check for `null` instead.

### Fixed

- Enum converters are registered by a factory instead of one line per enum. The old list was a trap:
  omitting an entry compiled, passed every test that did not exercise that field, and failed only
  against the real API — which is exactly how `SaleType` was found.
- `EventsResource` formats `from`/`to` as zoned UTC instants. The API rejects `2026-08-01` *and*
  `2026-08-01T00:00:00` with a bare `400 Bad Request` naming no field; only `…T00:00:00Z` is
  accepted, and local times are converted rather than relabelled so a window cannot silently shift.

## 2.0.0 — 2026-07-26

Breaking: auth is now owned by the integrator, not the SDK.

- `School21Client` no longer stores credentials or refreshes tokens internally. It takes an
  `ISchool21AccessTokenProvider` and reads a bearer from it before each request.
- New stateless `School21AuthClient` with `AuthenticateAsync` (password grant) and `RefreshAsync`
  (`refresh_token` grant), returning a `School21Token` (access/refresh tokens, lifetimes, absolute expiry).
- New `ISchool21AccessTokenProvider` with `StaticAccessTokenProvider` and `DelegateAccessTokenProvider`
  adapters; implement it to own token caching/refresh outside the SDK.
- `School21ClientOptions` reduced to `BaseUrl`; auth endpoints moved to `School21AuthOptions`.
- Removed the `AddSchool21Net` DI extension and the `Microsoft.Extensions.Http` dependency — the package is
  now dependency-free; wire the clients with your own container.

### Migrating from 1.x

```csharp
// 1.x
services.AddSchool21Net(new School21ClientOptions { Username = login, Password = password });

// 2.x — obtain tokens and refresh on your side (see README for a ready-made provider)
var auth = new School21AuthClient(httpClient);
var provider = new School21TokenProvider(auth, login, password); // your ISchool21AccessTokenProvider
var client = new School21Client(httpClient, new School21ClientOptions(), provider);
```

## 1.0.0 — 2026-07-26

- Initial release: typed client for the School 21 public API.
- ROPC password-grant auth (`client_id=s21-open-api`) with transparent token caching/refresh.
- Resources: `Participants` (basic info, projects, single project, coalition, points, feedback),
  `Projects` (per-project participant lists filterable by status/campus),
  `Campuses` (list, participants, coalitions), `Coalitions` (participants).
- Tolerant `SCREAMING_SNAKE` enum converters that raise on unknown values.
- Layered exceptions: `School21ApiException` / `School21ProtocolException` /
  `School21TransportException` / `School21ValidationException`.
