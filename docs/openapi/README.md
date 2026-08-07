# The School 21 OpenAPI specification

[`school21-openapi.json`](school21-openapi.json) is the spec as the platform serves it, kept here so
that adding an endpoint is reading rather than guessing.

**Fetch it from:** `https://platform.21-school.ru/services/21-school/api/swagger`

It is served **unauthenticated** and as JSON despite the name — no `/v3/api-docs`, no
`/swagger/v1/swagger.json`, no Swagger UI. Those all 404, which is what makes this URL worth writing
down: it is not one of the paths you would try.

Refresh with:

```sh
curl -s https://platform.21-school.ru/services/21-school/api/swagger \
  | python3 -m json.tool --sort-keys > docs/openapi/school21-openapi.json
```

## Coverage

All **25** paths in the spec have a method on the client. Checked mechanically against this file, not
by hand.

| Resource | Endpoints |
| --- | --- |
| `Participants` | profile, projects, project, courses, course, coalition, points, feedback, badges, skills, workstation, logtime, experience-history |
| `Campuses` | all, participants, coalitions, clusters |
| `Projects` | project, participants |
| `Coalitions` | participants |
| `Clusters` | map |
| `Courses` | course |
| `Events` | events |
| `Graph` | graph |
| `Sales` | sales |

## Things the spec states that are easy to get wrong

**`/v1/events` needs a zoned instant.** `from` and `to` are `date-time`, and the API rejects
`2026-08-01` *and* `2026-08-01T00:00:00` with a bare `400 Bad Request` naming no field. Only
`2026-08-01T00:00:00Z` is accepted. `EventsResource` formats them, so a caller cannot hit this — that
400 is otherwise unreadable, and the parameter names look right while failing.

**Events have no campus parameter.** The feed is scoped to the campus of the account the token
belongs to. Serving several campuses needs an account in each.

**An event's `type` is not the `type` filter.** The filter takes `ACTIVITY | EXAM | TEST`; the `type`
on a returned event is free text the campus writes — `Клуб`, `Митап`, `Мероприятие`. Two vocabularies
sharing one name. Do not map one onto the other.

**`logtime` returns a bare number**, not an object, so the client returns `double` rather than
inventing a wrapper the API does not have.

**Every wire enum needs a converter registered** in `School21Client`. A missing registration compiles
and passes every test that does not exercise that field, then throws the first time the value arrives
from the real API.
