# Changelog

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
