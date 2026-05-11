# SlotSmart – Execution Plan

> Master index for the phased delivery plan of the SlotSmart tennis club SaaS platform.
> Source of truth for product scope: [`../project-description.md`](../project-description.md).

---

## 1. How this plan is organized

The plan is split into three **release groupings** and seven **phases**. Each phase has its own folder with:

- A `README.md` describing the phase's goals, scope, dependencies, and acceptance criteria.
- One or more `task-NN-*.md` files, each a self-contained, agent-ready prompt that can be handed off to a coding agent (or a developer) and executed independently.

```text
docs/plan/
├── README.md                       ← you are here
├── 00-architecture/                ← cross-cutting architecture decisions
├── _templates/                     ← reusable task-prompt template
├── phase-1-foundation/             ← scaffolding, devops, repo layout
├── phase-2-auth-tenancy/           ← auth, multi-tenancy, RBAC
├── phase-3-club-members/           ← club + member domain
├── phase-4-training/               ← training sessions + schedules
├── phase-5-booking/                ← bookings + waiting lists
├── phase-6-notifications/          ← notifications + communication
└── phase-7-future/                 ← future / V2+ features (outline only)
```

Read the architecture docs in `00-architecture/` **before** picking up any task. Every task prompt assumes that context.

---

## 2. Release groupings

### Release 1.0 — MVP ("a small tennis club can run on this")

Phases 1–5. The goal is a usable product for **one or many** tennis clubs:

- Sign up a club and its admin
- Invite/register members (players, parents, coaches, head coaches, admins)
- Manage parent–child relationships
- Create training sessions (one-off and recurring) with capacity and assigned coaches
- Players/parents can book and cancel lessons; waiting lists are supported
- Coaches can mark attendance

> When phases 1–5 are done, SlotSmart should be deployable to a single environment and ready for a pilot club.

### Release 1.1 — Communication polish

Phase 6. Adds the communication layer (notifications, reminders, schedule updates) that takes the MVP from "functional" to "delightful."

### Release 2.0+ — Future

Phase 7 catalogs future capabilities (court reservations, payments, mobile app, AI scheduling, analytics, tournaments, coach performance tracking). These are intentionally **not** detailed at task level yet — they will be planned closer to the time, after MVP learnings.

---

## 3. Phase overview & dependencies

| # | Phase | Release | Depends on | Headline outcome |
|---|-------|---------|------------|------------------|
| 1 | [Foundation](./phase-1-foundation/README.md) | 1.0 | — | Repo, solution, DB, Docker, CI, frontend shell all run locally with `docker compose up`. |
| 2 | [Auth & Multi-Tenancy](./phase-2-auth-tenancy/README.md) | 1.0 | 1 | A user can sign up, sign in, and be scoped to a single tenant (club). RBAC enforced end-to-end. |
| 3 | [Club & Member Management](./phase-3-club-members/README.md) | 1.0 | 2 | Club admins can manage members, roles, coaches, and parent–child links. |
| 4 | [Training Management](./phase-4-training/README.md) | 1.0 | 3 | Coaches/admins can create one-off and recurring trainings with capacity and attendance. |
| 5 | [Booking System](./phase-5-booking/README.md) | 1.0 | 4 | Players/parents can book, cancel, and join waiting lists; availability is enforced. |
| 6 | [Communication & Notifications](./phase-6-notifications/README.md) | 1.1 | 5 | Outbound email + in-app notifications for bookings, reminders, schedule changes. |
| 7 | [Future / V2+](./phase-7-future/README.md) | 2.0+ | 6 | Court reservations, mobile, AI, payments, analytics, tournaments (outline only). |

### Dependency graph

```text
Phase 1 ──► Phase 2 ──► Phase 3 ──► Phase 4 ──► Phase 5 ──► Phase 6 ──► Phase 7+
                          │             │            │
                          └─ parent/    └─ coach     └─ player
                             child        scheduling   booking
```

Each phase is sequential at the **phase** level. Inside a phase, multiple tasks can often run in parallel — each task's prompt declares its own dependencies.

---

## 4. Cross-cutting documents (read first)

Before assigning any task to an agent, make sure the agent has read:

1. [`00-architecture/tech-stack.md`](./00-architecture/tech-stack.md) — the canonical tech choices.
2. [`00-architecture/solution-structure.md`](./00-architecture/solution-structure.md) — Clean Architecture project layout.
3. [`00-architecture/multi-tenancy-strategy.md`](./00-architecture/multi-tenancy-strategy.md) — tenant resolution, data isolation.
4. [`00-architecture/coding-standards.md`](./00-architecture/coding-standards.md) — naming, IDs, validation, errors, logging, testing.
5. [`00-architecture/api-conventions.md`](./00-architecture/api-conventions.md) — REST shape, pagination, errors, versioning.
6. [`00-architecture/domain-glossary.md`](./00-architecture/domain-glossary.md) — entities, terms, invariants.

The [`_templates/task-prompt-template.md`](./_templates/task-prompt-template.md) is the canonical shape for every handoff prompt.

---

## 5. How to hand off a task to an agent

Each task file is written so that a fresh agent (with only the repo + the architecture docs) can complete it. The recommended handoff message is:

> "Read `docs/plan/00-architecture/*.md` and `docs/plan/phase-N-<phase>/task-MM-<name>.md`. Execute that task end-to-end, following the *Acceptance Criteria* and *Definition of Done*. Update `CHANGELOG.md` per the project rules. Stop and ask before deviating from the architecture docs."

The shell of every task contains:

- **Context** – what the user, product, and architecture care about
- **Goal** – the one-sentence outcome
- **Scope (in / out)** – clear boundaries
- **Inputs** – files, env vars, API contracts the agent needs
- **Deliverables** – exact files, endpoints, screens to produce
- **Acceptance Criteria** – externally observable behavior
- **Definition of Done** – tests, docs, CI green, CHANGELOG entry
- **Out of scope / Future** – explicit non-goals
- **Handoff notes** – pitfalls, conventions, references

---

## 6. Conventions for this plan

- Phase folders are numbered with a one-digit prefix (`phase-1-…`).
- Tasks inside a phase are numbered with a two-digit prefix (`task-01-…`) so they sort naturally and so dependencies are visually obvious.
- Every task has a stable ID: `P{phase}-T{task}` (e.g. `P3-T02`). Use that ID when referencing tasks elsewhere.
- Tasks should each take a focused agent run (think: one PR-sized unit of work).
- "Future" / "V2+" features live in [`phase-7-future/README.md`](./phase-7-future/README.md). Do not plan them at task level yet.

---

## 7. Change log for the plan itself

The plan is a living document. When you add/modify a task, also append a one-line entry in the root `CHANGELOG.md` under `Changed → Plan`. Follow [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).
