# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) once the first release is tagged.

## [Unreleased]

### Added

- **P1-T01 — .NET 10 Clean Architecture solution scaffold** (`backend/`):
  - `backend/SlotSmart.slnx` (modern .NET 10 XML solution format) with 10 projects.
  - `backend/global.json` pins .NET 10 SDK 10.0.300 with `latestFeature` rollForward (per ADR-006).
  - `backend/Directory.Build.props` — solution-wide strict defaults: `net10.0`, nullable on, `TreatWarningsAsErrors=true`, `GenerateDocumentationFile=true` (so IDE0005 fires), latest analysers, deterministic build. Documented suppressions: CS1591, CA1716 (VB.NET keyword collisions on `Shared` / `Error`), CA1000 (`Result<T>.Success` / `Failure` factories).
  - `backend/Directory.Packages.props` — Central Package Management; every `<PackageReference>` in `src/` and `tests/` omits `Version=`.
  - `backend/.editorconfig` — file-scoped namespaces, nullable diagnostics escalated to error, IDE0005 / IDE0161 enforced.
  - `backend/tests/Directory.Build.props` — test projects relax strictness for analyser noise that doesn't survive contact with assertion frameworks.
  - **src/SlotSmart.Shared**: `Errors/Error` (RFC-7807-aligned record + factories), `Results/Result` and `Results/Result<T>` (functional outcome types — no exceptions for expected failures), `Identifiers/UuidV7` (RFC 9562, server-generated, stack-allocated, `bigEndian` Guid).
  - **src/SlotSmart.Domain**: `Common/Entity` base class implementing the dual-key identity pattern (ADR-007); marker interfaces `IAggregateRoot`, `IDomainEvent`, `IMultiTenantEntity { long TenantId }`. Project has zero non-BCL dependencies; layer-rule tests would fail the build if that changed.
  - **src/SlotSmart.Application** + **src/SlotSmart.Infrastructure**: `DependencyInjection.cs` extension methods (`AddSlotSmartApplication` / `AddSlotSmartInfrastructure`) — empty stubs the API composition root calls today; handlers / DbContext / interceptors land in P1-T02 onwards.
  - **src/SlotSmart.Api**: minimal-API `Program.cs` exposing `GET /api/v1/health → 200 {"status":"ok"}` on port 5080 (HTTP) / 7080 (HTTPS); `MapOpenApi()` wired (Development env only — full document & client generation in P1-T06).
  - **tests/SlotSmart.Architecture.Tests** (NetArchTest + FluentAssertions):
    - `LayerRulesTests` — Domain has no dependency on Application/Infrastructure/Api or on `Microsoft.EntityFrameworkCore`/`Microsoft.AspNetCore`/`Microsoft.Extensions.Hosting`; Application doesn't see Infrastructure or Api; Infrastructure doesn't see Api; Api is the only composition root.
    - `EntityIdentityRulesTests` — every concrete persistent entity inherits from `Entity`; `EntityId` has no public setter; `IMultiTenantEntity.TenantId` is `long`; no `[Key]` on `EntityId`.
    - **Red-green demo logged in completion report**: temporarily added `Microsoft.EntityFrameworkCore` reference to Domain → `Domain_does_not_depend_on_external_libraries_like_EFCore_or_AspNetCore` failed with `offending types: SlotSmart.Domain.Temp.TempViolation`. Reverted → 9/9 passes.
  - **tests/SlotSmart.{Domain,Application,Infrastructure,Api}.Tests**: each has a real first test (Entity equality semantics in Domain; DI registration in Application/Infrastructure; WAF-based `/api/v1/health` smoke in Api). 15 tests total, all green.
  - **Result of `dotnet build SlotSmart.slnx -warnaserror`**: 0 warnings, 0 errors, 10 projects.
  - **Result of `dotnet test`**: 5/5 test assemblies pass; 15/15 tests pass.
  - **Live API smoke**: `dotnet run --project src/SlotSmart.Api --urls http://localhost:5080` + `curl -fsS http://localhost:5080/api/v1/health` → `200 {"status":"ok"}`.

- **Repo root**: `README.md` (overview, .NET 10 install instructions, build & run, conventions, where-to-read-next) and `.gitignore` (Visual Studio / Rider / macOS / Node / .env / coverage / Docker / NuGet) added.

- **Model selection guidance** for worker handoffs:
  - `handoff-prompts/_template.md` now requires a "Dispatch metadata" block (suggested model + rationale + escalation rule) above the copy-to-worker markers.
  - `handoff-prompts/README.md` documents the **Opus / Sonnet / Auto** selection table with task-type guidance and explicit escalation rules.
  - `DELEGATION-TRACKER.md` v2 inventory tables now include a **Model** column for all 33 in-scope tasks, plus a model-distribution summary (Opus: 7, Sonnet: 19, Auto: 7).
  - `handoff-prompts/P1-T01-solution-scaffolding.md` updated with dispatch metadata: **Sonnet**, with documented escalation paths to Opus.

- **Supervised execution infrastructure** for the MVP plan (Phases 1–5), under `docs/SUPERVISOR/`:
  - `EXECUTION-PLAN.md` — strategy, sequencing, parallelism rules, risks (R1–R6, D1–D2), working agreements, and a per-session checklist.
  - `phase-gates/` — verification specs that gate phase advancement: `README.md`, `phase-1-gate.md`, `phase-2-gate.md`, `phase-3-gate.md`, `phase-4-gate.md`, `phase-5-gate.md`. Each contains pre-conditions, automated shell-verification commands with expected outputs, manual checks, a promotion checklist, and a run log.
  - `handoff-prompts/` — worker handoff prompt infrastructure: `README.md`, `_template.md`, and the first ready-to-dispatch prompt `P1-T01-solution-scaffolding.md`.
  - `DELEGATION-TRACKER.md` rewritten (v2) to inventory all 33 in-scope tasks across Phases 1–5 with size, dependencies, status, and owning phase gate.
  - `CURRENT-STATUS.md` rewritten (v2) to dashboard mission progress (0/33 tasks, 0/5 gates) and surface open decisions (R1: .NET version pin; execution mode for P1-T01).
  - `THINKING-LOG.md` entry "Supervised Execution Plan for MVP" recording the just-in-time handoff strategy.
  - `DECISIONS-LOG.md` ADR-004 (phase gates as checked-in markdown) and ADR-005 (just-in-time handoff prompt generation). ADR-006 reserved for the .NET-version pin decision.

### Changed

- **DECISIONS-LOG**: ADR-006 documents the .NET 10 GA pin (10.0.300, LTS) — risks R1 and R2 in `EXECUTION-PLAN.md` are now closed. ADR-007 ratifies the dual-key identity pattern (entry was added concurrently with the `entity-identity.md` doc); ADR-003 (UUIDv7 PK) is now `Superseded by ADR-007`.
- **DELEGATION-TRACKER**: P1-T01 moved to `✅ Recently Completed` (HYBRID mode, SUPERVISOR self-executed). P1-T02 / P1-T04 / P1-T05 / P1-T07 are now unblocked and queued for handoff-prompt generation.
- **CURRENT-STATUS**: Phase 1 progress 1/7 tasks; backend scaffold component flipped to `✅ Complete`; open-decisions list reduced (R1 / R2 / .NET pin / execution mode resolved).
- **Plan / Architecture**: switched entity identity to a **dual-key pattern** — every entity now has a hidden `bigint` surrogate primary key (the actual clustered / B-tree PK) and a separate public `EntityId : Guid` (UUIDv7) for external use. Motivated by SQL Server's clustered-key inclusion cost and the size / join cost of 16-byte GUIDs across both Postgres and SQL Server (random GUIDs are catastrophic; sequential UUIDv7 mitigates fragmentation but not size). Foreign keys reference the surrogate `bigint`; domain code uses navigation properties. The Domain layer remains pure — the surrogate is an EF Core shadow property added in Infrastructure. Multi-tenant tables now carry `TenantId : bigint` (FK to `Tenants.Id`) instead of `TenantId : uuid`, and `ITenantContext` carries both forms.
  - Added: `docs/plan/00-architecture/entity-identity.md` (canonical pattern, naming, EF configuration, FK strategy, Identity tables, hot-table guidance, DB specifics, architecture tests).
  - Updated: `docs/plan/00-architecture/coding-standards.md` — new "Identifiers" section; UUIDv7 rule reframed as the public id only.
  - Updated: `docs/plan/00-architecture/domain-glossary.md` — Identifiers & time section.
  - Updated: `docs/plan/00-architecture/multi-tenancy-strategy.md` — `TenantId` column is `bigint`; `ITenantContext` carries both surrogate and Guid.
  - Updated: `docs/plan/00-architecture/solution-structure.md` — added Entity base type section.
  - Updated: `docs/plan/README.md` — `entity-identity.md` added to the read-first list, with a global interpretation note for aggregate field lists in task prompts (`Id` in a task means the public `EntityId : Guid`).
  - Existing task prompts in `phase-1`..`phase-6` continue to list aggregate fields as `Id, TenantId, …`. Per the README's interpretation note, agents read those as `EntityId : Guid` and `TenantId : long` respectively; no per-task edits required.

#### Notes (background)

The plan files in `docs/plan/` describe **what** to build per phase. The supervised execution infrastructure adds **how to safely advance**: every phase boundary now has a copy-paste verification spec, every worker dispatch has a context-aware handoff prompt, and every task is tracked from queued → dispatched → in-review → done with explicit gate accountability. This is the bridge from "well-written plan" to "actually shippable MVP".

#### Files added (supervised execution infra)

- `docs/SUPERVISOR/EXECUTION-PLAN.md`
- `docs/SUPERVISOR/phase-gates/README.md`
- `docs/SUPERVISOR/phase-gates/phase-1-gate.md` … `phase-5-gate.md`
- `docs/SUPERVISOR/handoff-prompts/README.md`
- `docs/SUPERVISOR/handoff-prompts/_template.md`
- `docs/SUPERVISOR/handoff-prompts/P1-T01-solution-scaffolding.md`

#### Files changed (supervised execution infra)

- `docs/SUPERVISOR/DELEGATION-TRACKER.md` (v1 → v2: full P1–P5 inventory)
- `docs/SUPERVISOR/CURRENT-STATUS.md` (v1 → v2: mission dashboard)
- `docs/SUPERVISOR/THINKING-LOG.md` (added 2026-05-12 strategy entry)
- `docs/SUPERVISOR/DECISIONS-LOG.md` (added ADR-004, ADR-005; reserved ADR-006)

---

- **AI Supervisor-Worker Framework** initialized in `docs/SUPERVISOR/` for structured AI-assisted development.
  - Framework core files: `SUPERVISOR-FRAMEWORK.md`, `WORKER-FRAMEWORK.md`, `BEST-PRACTICES.md`.
  - Project tracking files: `CURRENT-STATUS.md`, `DELEGATION-TRACKER.md`, `THINKING-LOG.md`, `DECISIONS-LOG.md`.
  - Template files: `HANDOVER-TEMPLATE.md`, `BUG-FIX-TEMPLATE.md`, `EMERGENCY-PROTOCOL.md`.
  - Session archive directory: `SESSION-SUMMARIES/`.
- **AI IDE configuration files** in project root: `CLAUDE.md`, `context.md`, `.cursorrules`, `.windsurfrules`.
  - Customized for SlotSmart tech stack (ASP.NET Core, Clean Architecture, React, MUI).
  - Includes domain rules, multi-tenant safety rules, coding conventions.
- **Methodology skills** installed in `.cursor/skills/` for Cursor IDE:
  - `init-supervisor` — Bootstrap/resume Supervisor sessions with situational briefing.
  - `brainstorming` — Structured design exploration before implementation.
  - `writing-plans` — Comprehensive implementation planning with TDD task decomposition.
  - `test-driven-development` — Red-green-refactor discipline with C# xUnit examples.
  - `systematic-debugging` — Four-phase root-cause debugging methodology.
  - `requesting-code-review` — Code review workflow with SlotSmart-specific checklist.
  - `verification-before-completion` — Evidence-based completion verification.
  - Supporting files: `root-cause-tracing.md`, `defense-in-depth.md`, `condition-based-waiting.md`.
- **Cursor methodology bridge rule** at `.cursor/rules/superpowers-methodology.md` mapping Supervisor activities to skills.
- **Architecture Decision Records** (ADR-001, ADR-002, ADR-003) in `DECISIONS-LOG.md` documenting framework adoption, ASP.NET Core + Clean Architecture, and UUIDv7 identifier choices.

- **Plan**: full phased execution plan under `docs/plan/`.
  - Master plan index `docs/plan/README.md` with release groupings (MVP, V1.1, V2+) and phase dependency map.
  - Cross-cutting architecture docs under `docs/plan/00-architecture/`:
    - `tech-stack.md` — canonical tech choices (ASP.NET Core 10, EF Core, Postgres, React 19, MUI, Vite).
    - `solution-structure.md` — Clean Architecture layout and module organization.
    - `multi-tenancy-strategy.md` — shared-schema row-level tenant model, resolution order, query filters.
    - `coding-standards.md` — language conventions, testing, commits.
    - `api-conventions.md` — REST shape, errors (RFC 7807), pagination, idempotency, OpenAPI.
    - `domain-glossary.md` — entities, terms, invariants.
  - Reusable task-prompt template at `docs/plan/_templates/task-prompt-template.md`.
  - **Phase 1 — Foundation** (MVP) with 7 agent-ready task prompts: solution scaffolding, EF Core + Postgres, Docker compose, CI, frontend scaffolding, OpenAPI + generated client, observability.
  - **Phase 2 — Auth & Multi-Tenancy** (MVP) with 6 tasks: tenant resolution + query filters, OpenIddict + JWT, RBAC, club signup, frontend auth flow, audit log.
  - **Phase 3 — Club & Member Management** (MVP) with 7 tasks: club settings, Member CRUD + invitations, role management, coach profile, parent–child relations, frontend members + coach/parent UI.
  - **Phase 4 — Training Management** (MVP) with 7 tasks: Training aggregate, TrainingSeries + materialization, edit/cancel scope rules, coach overlap, attendance, calendar UI, attendance UI.
  - **Phase 5 — Booking System** (MVP) with 6 tasks: Booking aggregate + state machine, waiting list + auto-promote, availability/window enforcement, booking↔attendance integration, transactional outbox, frontend booking + my bookings.
  - **Phase 6 — Communication & Notifications** (V1.1) with 7 tasks: notification model + dispatcher, Email channel (SMTP + templating), in-app channel, user preferences, event handlers, scheduled reminders, frontend notifications UI.
  - **Phase 7 — Future / V2+** outline-only document covering mobile app, AI scheduling, slot optimization, court reservations, analytics, payments, tournaments, coach performance tracking, plus cross-cutting V2 candidates.

#### Notes (phased plan)

A clearly phased, agent-ready plan lets us hand off independent units of work — each task is a self-contained PR-sized prompt with context, scope, acceptance criteria, and a definition of done — and keeps later phases (notifications, future features) on a known dependency path.

#### Files added (phased plan)

- `docs/plan/README.md`
- `docs/plan/00-architecture/{tech-stack,solution-structure,multi-tenancy-strategy,coding-standards,api-conventions,domain-glossary}.md`
- `docs/plan/_templates/task-prompt-template.md`
- `docs/plan/phase-1-foundation/README.md`
- `docs/plan/phase-1-foundation/task-01-solution-scaffolding.md`
- `docs/plan/phase-1-foundation/task-02-database-ef-core.md`
- `docs/plan/phase-1-foundation/task-03-docker-compose.md`
- `docs/plan/phase-1-foundation/task-04-ci-pipeline.md`
- `docs/plan/phase-1-foundation/task-05-frontend-scaffolding.md`
- `docs/plan/phase-1-foundation/task-06-openapi-and-client.md`
- `docs/plan/phase-1-foundation/task-07-observability.md`
- `docs/plan/phase-2-auth-tenancy/README.md`
- `docs/plan/phase-2-auth-tenancy/task-01-tenant-resolution.md`
- `docs/plan/phase-2-auth-tenancy/task-02-auth-openiddict.md`
- `docs/plan/phase-2-auth-tenancy/task-03-rbac.md`
- `docs/plan/phase-2-auth-tenancy/task-04-club-signup.md`
- `docs/plan/phase-2-auth-tenancy/task-05-frontend-auth.md`
- `docs/plan/phase-2-auth-tenancy/task-06-audit-log.md`
- `docs/plan/phase-3-club-members/README.md`
- `docs/plan/phase-3-club-members/task-01-club-settings.md`
- `docs/plan/phase-3-club-members/task-02-member-crud.md`
- `docs/plan/phase-3-club-members/task-03-member-roles.md`
- `docs/plan/phase-3-club-members/task-04-coach-profile.md`
- `docs/plan/phase-3-club-members/task-05-parent-child.md`
- `docs/plan/phase-3-club-members/task-06-frontend-members.md`
- `docs/plan/phase-3-club-members/task-07-frontend-coach-parent.md`
- `docs/plan/phase-4-training/README.md`
- `docs/plan/phase-4-training/task-01-training-aggregate.md`
- `docs/plan/phase-4-training/task-02-training-series.md`
- `docs/plan/phase-4-training/task-03-edit-cancel-rules.md`
- `docs/plan/phase-4-training/task-04-coach-overlap.md`
- `docs/plan/phase-4-training/task-05-attendance.md`
- `docs/plan/phase-4-training/task-06-frontend-calendar.md`
- `docs/plan/phase-4-training/task-07-frontend-attendance.md`
- `docs/plan/phase-5-booking/README.md`
- `docs/plan/phase-5-booking/task-01-booking-aggregate.md`
- `docs/plan/phase-5-booking/task-02-waiting-list.md`
- `docs/plan/phase-5-booking/task-03-availability.md`
- `docs/plan/phase-5-booking/task-04-booking-attendance.md`
- `docs/plan/phase-5-booking/task-05-outbox.md`
- `docs/plan/phase-5-booking/task-06-frontend-booking.md`
- `docs/plan/phase-6-notifications/README.md`
- `docs/plan/phase-6-notifications/task-01-notification-model.md`
- `docs/plan/phase-6-notifications/task-02-email-channel.md`
- `docs/plan/phase-6-notifications/task-03-in-app-channel.md`
- `docs/plan/phase-6-notifications/task-04-preferences.md`
- `docs/plan/phase-6-notifications/task-05-event-handlers.md`
- `docs/plan/phase-6-notifications/task-06-reminders.md`
- `docs/plan/phase-6-notifications/task-07-frontend.md`
- `docs/plan/phase-7-future/README.md`
- `CHANGELOG.md` (this file)
