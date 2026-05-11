# Phase 1 — Foundation

> Release: **1.0 (MVP)** · Depends on: nothing · Unlocks: every subsequent phase.

## Goal

Make the project **runnable** end-to-end with `docker compose up` and ready for feature work. No business features yet — just the scaffolding, the database, CI, and a working "hello world" path from React → API → Postgres.

## Outcomes (at the end of this phase)

- A .NET 10 solution following the Clean Architecture layout from [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md).
- EF Core + Postgres wired up, with the first migration applied automatically in dev.
- A Vite + React + TypeScript + MUI app that calls a `/api/v1/health` endpoint and renders the response.
- `docker compose up` starts: `postgres`, `api`, `web`, `seq` (logs).
- GitHub Actions runs build + test + lint on every PR.
- `CHANGELOG.md` exists and is followed.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P1-T01 | [Repository & solution scaffolding](./task-01-solution-scaffolding.md) | M | — |
| P1-T02 | [Database, EF Core, migrations bootstrap](./task-02-database-ef-core.md) | M | P1-T01 |
| P1-T03 | [Docker & docker-compose dev environment](./task-03-docker-compose.md) | M | P1-T01, P1-T02 |
| P1-T04 | [CI pipeline (GitHub Actions)](./task-04-ci-pipeline.md) | S | P1-T01 |
| P1-T05 | [Frontend scaffolding (Vite + MUI + Router)](./task-05-frontend-scaffolding.md) | M | P1-T01 |
| P1-T06 | [OpenAPI + generated TS client + shared health endpoint](./task-06-openapi-and-client.md) | M | P1-T01, P1-T05 |
| P1-T07 | [Observability baseline (Serilog + OpenTelemetry + Seq)](./task-07-observability.md) | S | P1-T01 |

P1-T01 must finish first. After it lands, the rest can largely run in parallel; P1-T03 needs T02 and T05 to be in place.

## Acceptance criteria for the whole phase

- [ ] Fresh clone + `docker compose up` → a developer sees the React app rendering the health response from the API, with the API reading from Postgres.
- [ ] `dotnet build` and `dotnet test` succeed at the repo root.
- [ ] `npm run build`, `npm run test`, `npm run lint` succeed in `frontend/`.
- [ ] CI green on a pushed branch.
- [ ] CHANGELOG.md has entries for every task.
