# Changelog

## 1.0.0 — 2026-07-26

- Initial release: typed client for the School 21 public API.
- ROPC password-grant auth (`client_id=s21-open-api`) with transparent token caching/refresh.
- Resources: `Participants` (basic info, projects, single project, coalition, points, feedback),
  `Projects` (per-project participant lists filterable by status/campus),
  `Campuses` (list, participants, coalitions), `Coalitions` (participants).
- Tolerant `SCREAMING_SNAKE` enum converters that raise on unknown values.
- Layered exceptions: `School21ApiException` / `School21ProtocolException` /
  `School21TransportException` / `School21ValidationException`.
