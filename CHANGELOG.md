# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) once the first release is tagged.

## [Unreleased]

### Added

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

### Why this matters

A clearly phased, agent-ready plan lets us hand off independent units of work — each task is a self-contained PR-sized prompt with context, scope, acceptance criteria, and a definition of done — and keeps later phases (notifications, future features) on a known dependency path.

### Files added

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
