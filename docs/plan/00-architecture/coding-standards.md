# Coding Standards

## General

- **English only** in code, comments, commit messages, and docs.
- **Small files, single responsibility.** If a file grows past ~300 lines without a structural reason, split it.
- **No dead code.** Delete it; git is the history.
- **No commented-out code** in PRs. Use feature flags or branches if you must keep something alive.
- Comments explain **intent / why**, never restate the code.
- Public APIs and exported TypeScript symbols have XML / JSDoc comments where the name alone is insufficient.

## .NET / C#

- Nullable reference types **on** for all production projects.
- `TreatWarningsAsErrors=true` for all production projects.
- Prefer `record` / `record struct` for DTOs and value objects.
- Prefer **expression-bodied** members for trivial properties/methods.
- File-scoped namespaces.
- `var` is allowed when the type is obvious from the right-hand side; otherwise use the explicit type.
- One public type per file. File name == type name.
- Async methods end with `Async` and accept `CancellationToken` as the last parameter.
- Don't `async void` except for event handlers.
- Use `Result<T>` (in `SlotSmart.Shared`) for expected failures; throw only for programmer errors and infrastructure faults.
- Use `IClock` (or `TimeProvider`) instead of `DateTime.UtcNow` directly.
- Use **UUIDv7** for the public `EntityId` on every entity. The database primary key is a hidden `bigint` surrogate — see [`entity-identity.md`](./entity-identity.md).

### Domain layer rules

- Entities have private setters; mutations go through methods that protect invariants.
- Value objects are immutable `record` types with private `Validate(…)`.
- Aggregates raise domain events via `RaiseEvent(…)`; the outbox dispatches them after commit.
- The Domain layer **never** references EF Core, ASP.NET Core, or `System.Net.*`.

### Application layer rules

- One command/query per use case. Each lives in its own folder with handler + validator + DTOs.
- Handlers are sealed and stateless.
- Validators (FluentValidation) validate **shape**; domain methods validate **invariants**. Don't duplicate.
- Use **MediatR pipeline behaviors** for: validation, logging, transactions (unit of work), and authorization. Do not duplicate that logic inside handlers.

## TypeScript / React

- `strict: true` in `tsconfig.json`; no `any` except behind well-named adapters.
- Prefer **named exports**; reserve default exports for pages/route components.
- Components are **function components**; no class components.
- One component per file; file name in `PascalCase.tsx`.
- Hooks live in `useXxx.ts` files; only one hook per file unless the hooks are trivially related.
- Use **TanStack Query** for server state; do not store server data in React state or Zustand.
- Forms: **React Hook Form** + **Zod** schema. Schema is the source of truth for the form type.
- API calls go through the generated client only; no ad-hoc `fetch`/`axios` in feature code.
- Use Material UI's `sx` prop and theme tokens; avoid inline `style={{ … }}` for layout.
- Translations: every visible string goes through `useTranslation`. English keys; do not concatenate translated strings.

## Errors and logging

- Backend: log at the boundary (middleware) with structured fields: `traceId`, `tenantId`, `userId`, `requestPath`. Don't log inside happy-path handlers.
- Never log PII (email, phone, child names) at `Information` or above; tag those fields and scrub them.
- Frontend: surface errors as MUI snackbars + inline messages; never show raw stack traces.

## API errors

- Use `application/problem+json` (RFC 7807) for all error responses.
- `type` is a stable code (e.g. `slotsmart/errors/booking-full`). `title` is human, `detail` is human + safe.
- Validation errors return `400` with `errors: { field: [message, …] }`.

## Testing

- Unit tests for Domain and Application — fast, no I/O.
- Integration tests for Infrastructure — Testcontainers Postgres.
- Endpoint tests for Api — `WebApplicationFactory`, real DB via Testcontainers.
- Test naming: `Method_State_ExpectedResult` (e.g. `Book_WhenTrainingFull_AddsToWaitingList`).
- Use FluentAssertions: `.Should().Be(…)`.
- Aim for **70%+** coverage of Domain + Application; do not chase coverage in Infrastructure.

## Commits & PRs

- Conventional Commits: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`, `build:`, `ci:`.
- PRs are small and focused (target one task from the plan per PR).
- PR description must reference the task ID (e.g. `Implements P3-T02`).
- CHANGELOG.md is updated in the same PR per the project's Development Rules (Keep a Changelog 1.1.0).

## Identifiers

- **Every entity uses the dual-key pattern** defined in [`entity-identity.md`](./entity-identity.md):
  - A hidden `bigint` surrogate PK (`Id`, EF Core shadow property) — the actual database primary key / clustered index.
  - A public `EntityId : Guid` (UUIDv7) — the only id the domain, API, JWT, audit log, and clients ever see.
- Generate UUIDv7 server-side via the helper in `SlotSmart.Shared.Identifiers.UuidV7`.
- Never expose the surrogate `Id` over the wire. Public JSON serializes `EntityId` as `id`.
- Foreign keys reference the surrogate `bigint` by default; domain code expresses relationships via **navigation properties**, not Guid columns.

## Naming

- Tenants identified by `Slug` (URL-safe), all lowercase. `Tenant.EntityId : Guid` is the stable platform-wide id; `Tenants.Id : bigint` is the surrogate used by all multi-tenant FKs and the `ITenantContext.TenantId` filter.
- Time stored as `timestamp with time zone` (UTC). Display layer formats in local TZ.
- Money (when introduced in V2) uses `decimal(18,4)` with explicit currency code.
- Booleans are positively named: `IsActive`, not `IsNotActive`.
