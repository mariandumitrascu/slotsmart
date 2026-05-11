# API Conventions

## Base URL & versioning

- All API routes live under `/api/v1/...`.
- The `v1` segment is permanent for the MVP. Breaking changes will go to `/api/v2`.
- Tenant is **not** in the URL. It comes from the JWT or `X-Tenant-Slug` header (see [`multi-tenancy-strategy.md`](./multi-tenancy-strategy.md)).

## Resources

- Resource names are **plural nouns**: `/api/v1/members`, `/api/v1/trainings`, `/api/v1/bookings`.
- Sub-resources use nesting **one level deep**: `/api/v1/members/{memberId}/relations`. Past one level, use a flat resource.
- Actions that don't fit CRUD are sub-routes with verbs: `POST /api/v1/trainings/{id}:cancel`. Use the `:` prefix to make it obvious.

## HTTP methods

| Method | Use |
|---|---|
| GET | Read; never mutates state. |
| POST | Create; or action sub-routes (`:cancel`, `:publish`). |
| PUT | Full replace. Rarely used. |
| PATCH | Partial update with JSON Merge Patch (`application/merge-patch+json`). |
| DELETE | Remove. Soft delete by default; hard delete only on platform admin routes. |

## Status codes

| Code | When |
|---|---|
| 200 OK | Successful GET / PATCH / action that returns a body. |
| 201 Created | Successful POST that creates a resource. `Location` header set. |
| 204 No Content | Successful DELETE or PATCH with no body. |
| 400 Bad Request | Validation errors; problem+json with `errors`. |
| 401 Unauthorized | Missing or invalid credentials. |
| 403 Forbidden | Authenticated but not allowed (role/policy). |
| 404 Not Found | Resource does not exist **or** caller cannot see it (do not leak existence across tenants). |
| 409 Conflict | Concurrency conflict or domain conflict (e.g. duplicate slug). |
| 422 Unprocessable Entity | Domain invariant violation (e.g. booking when training full and no waitlist). |
| 500 Internal Server Error | Unexpected. Never includes stack traces. |

## Error format (RFC 7807 problem+json)

```json
{
  "type": "slotsmart/errors/booking-full",
  "title": "Training session is full",
  "status": 422,
  "detail": "There are no remaining slots and waiting list is disabled.",
  "instance": "/api/v1/trainings/01HV0…/bookings",
  "traceId": "00-…-…-01",
  "errors": {
    "playerId": ["Player is already booked into this training."]
  }
}
```

`type` is a stable, machine-readable error code. Frontend matches on `type`.

## Pagination

- Query: `?page=1&pageSize=20&sort=createdAt:desc`.
- Default `pageSize=20`, max `100`.
- Response shape:

```json
{
  "items": [ … ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 137,
  "totalPages": 7
}
```

For high-cardinality endpoints (audit logs, notifications), use **cursor pagination**:

```text
GET /api/v1/notifications?cursor=eyJ…&limit=50
```

Response includes `nextCursor` (nullable).

## Filtering

- Simple equality filters as flat query params: `?status=Active&coachId=…`.
- Date ranges: `?from=2026-01-01&to=2026-01-31` (ISO 8601, UTC).
- Free-text search: `?q=...`.
- No OData. No complex query DSL.

## Idempotency

- Mutating endpoints that may be retried (`POST /bookings`, `POST /trainings`, payment endpoints in V2) accept an `Idempotency-Key` header. The server stores `(key, tenant, hash)` for 24h and returns the cached response on retry.

## Concurrency

- Aggregates expose a `Version` (rowversion / xmin). Update endpoints require `If-Match: "<version>"`; mismatch returns 409.

## OpenAPI

- The API generates a complete **OpenAPI 3.1** document at `/api/v1/openapi.json` and serves Swagger UI at `/api/v1/docs` in non-production environments.
- The frontend client is generated from this document — it is the contract.
- Every endpoint must have a summary, description, and at least one example response per status code.

## Authentication

- `Authorization: Bearer <jwt>` for all protected endpoints.
- Refresh token exchange at `POST /api/v1/auth/token/refresh`.
- Public endpoints are explicitly opted in via `[AllowAnonymous]` and listed in the [`multi-tenancy-strategy.md`](./multi-tenancy-strategy.md).

## CORS

- Allowed origins configured per environment; never `*` in production.

## Rate limiting

- Per-tenant + per-IP rate limits via the built-in ASP.NET Core rate limiter. Tuned in Phase 1.
