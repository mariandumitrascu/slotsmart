# Task `P1-T01` — Repository & .NET Solution Scaffolding

> **Phase**: 1 — Foundation
> **Estimated size**: M
> **Depends on**: none
> **Can run in parallel with**: P1-T05 (frontend), once started

---

## 1. Context

The repository currently contains only `docs/`. We need to lay down the .NET solution following the Clean Architecture layout described in [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md). Subsequent tasks build on this layout, so getting it right matters.

## 2. Goal

> After this task, `cd backend && dotnet build` succeeds for a complete Clean Architecture solution with the seven projects and four test projects defined in the architecture doc, and architecture rules are enforced by tests.

## 3. Scope

### In scope

- Create the `backend/` folder, `SlotSmart.sln`, and the seven projects: `Domain`, `Application`, `Infrastructure`, `Api`, `Shared`, `Architecture.Tests`, plus tests `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`, `Api.Tests`.
- Set up `Directory.Build.props`, `Directory.Packages.props` (Central Package Management), `.editorconfig`, and a root `global.json` pinning .NET 10 SDK.
- Add `NetArchTest` (or Roslyn-based) tests asserting the layer rules from [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md).
- Add a minimal `Program.cs` in `SlotSmart.Api` that returns `200 OK { "status": "ok" }` on `GET /api/v1/health`.
- Wire up `Microsoft.Extensions.Hosting` + structured logging stub (real Serilog is P1-T07).
- Add a root `README.md` with "how to run" instructions for backend (DB and frontend are documented when those tasks finish).
- Add a `CHANGELOG.md` at the repo root following [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).
- Add `.gitignore` (Visual Studio + Rider + macOS + Node, plus `frontend/.env*`).

### Out of scope (handled by another task)

- EF Core / DbContext / migrations → **P1-T02**.
- Docker / docker-compose → **P1-T03**.
- CI pipeline → **P1-T04**.
- Frontend → **P1-T05**.
- OpenAPI + generated client → **P1-T06**.
- Serilog + OpenTelemetry full setup → **P1-T07**.

## 4. Inputs

- Architecture docs to read first:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- No external services required.
- .NET 10 SDK installed locally (or .NET 9 with daily SDK; pin via `global.json`).

## 5. Deliverables

### Backend

- `backend/SlotSmart.sln`
- `backend/global.json`, `backend/Directory.Build.props`, `backend/Directory.Packages.props`, `backend/.editorconfig`
- `backend/src/SlotSmart.Shared/SlotSmart.Shared.csproj` + a `Result<T>` primitive + `Error` record + UUIDv7 helper.
- `backend/src/SlotSmart.Domain/SlotSmart.Domain.csproj` (empty body; only abstractions/interfaces like `IAggregateRoot`, `IDomainEvent`, `IMultiTenantEntity`).
- `backend/src/SlotSmart.Application/SlotSmart.Application.csproj` + `DependencyInjection.cs` placeholder.
- `backend/src/SlotSmart.Infrastructure/SlotSmart.Infrastructure.csproj` + `DependencyInjection.cs` placeholder.
- `backend/src/SlotSmart.Api/SlotSmart.Api.csproj` + `Program.cs` exposing `GET /api/v1/health`.
- `backend/tests/SlotSmart.Architecture.Tests/` with at least these tests:
  - `Domain` has no project references.
  - `Application` references only `Domain` + `Shared`.
  - `Infrastructure` does not reference `Api`.
  - `Api` is the only composition root.
- `backend/tests/SlotSmart.Domain.Tests/`, `Application.Tests/`, `Infrastructure.Tests/`, `Api.Tests/` with one trivial passing test each (so test infra is wired).

### Repo root

- `README.md` (project intro + "how to run backend").
- `CHANGELOG.md` starter file.
- `.gitignore`, `.editorconfig` at root level if not already covered by `backend/.editorconfig`.

### Docs

- Update `CHANGELOG.md` under `Added` for: "Initial .NET 10 Clean Architecture solution scaffold (`backend/`)."

## 6. Acceptance Criteria

- [ ] `cd backend && dotnet restore && dotnet build` succeeds with **zero** warnings.
- [ ] `cd backend && dotnet test` runs all test projects and they pass.
- [ ] `cd backend && dotnet run --project src/SlotSmart.Api` serves `GET http://localhost:5080/api/v1/health` → `200 { "status": "ok" }`.
- [ ] `Architecture.Tests` fail if someone makes `Domain` reference EF Core (manually verify).
- [ ] Nullable reference types are enabled and `TreatWarningsAsErrors=true` is set in `Directory.Build.props` for all `src/` projects.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] `CHANGELOG.md` updated under `Added`.
- [ ] PR description references `P1-T01` and lists architecture decisions made.
- [ ] No `TODO` / `FIXME` left without a referenced follow-up.

## 8. Handoff notes / gotchas

- Use **Central Package Management** (`Directory.Packages.props`). All `<PackageReference>` entries omit versions; versions are centralized.
- The `Api` project is the only one with a `Program.cs` and an `IServiceCollection.AddSlotSmart…()` composition root.
- Don't add MediatR, FluentValidation, or EF Core yet — those come with the tasks that need them. Keep this PR small.
- The default Kestrel port for the API is `5080` (HTTP) / `7080` (HTTPS) in dev. Set in `launchSettings.json`.
- The Result type lives in `SlotSmart.Shared` and is **not** generic over error types in MVP — use a single `Error` record with `Code`, `Title`, `Detail`, and `Type` (the RFC 7807 stable code).

## 9. Suggested execution outline

1. `dotnet new sln -n SlotSmart` under `backend/`.
2. Create projects with `dotnet new classlib` (and `webapi` for Api), then `dotnet sln add`.
3. Wire project references following layer rules.
4. Add `Directory.Build.props` (common compile settings) and `Directory.Packages.props` (CPM).
5. Add `NetArchTest.Rules` to `Architecture.Tests` and write the layer-rule tests.
6. Add the `/api/v1/health` Minimal API endpoint.
7. Confirm `dotnet build && dotnet test` clean.
8. Write `README.md` and `CHANGELOG.md` and commit.

## 10. Open questions / risks

- Risk: .NET 10 SDK availability. **Mitigation**: pin via `global.json` and document the install step in `README.md`. If GA not released, use the latest preview SDK with `rollForward: latestPreview`.
- Question: do we need the `SlotSmart.Architecture.Tests` project from day one? **Decision**: yes — it costs little and prevents drift.
