# Handoff Prompt Template

> Copy this file to `P{n}-T{nn}-{slug}.md` and fill in the bracketed sections.
> Keep it lean — the canonical task spec lives in `docs/plan/`. This prompt only adds the worker initialization, current state, and supervision contract.

---

## Dispatch metadata (SUPERVISOR / user-facing — do **not** copy to the worker)

- **Suggested model**: `{Opus | Sonnet | Auto}`
- **Why this model**: `{1–2 sentences justifying the tier — see "Model selection" in handoff-prompts/README.md}`
- **Suggested escalation**: `{when to upgrade — e.g. "If the worker hits the .NET 10 install failure, switch to Opus to design the fallback ADR"}`

---

## ⤵ COPY EVERYTHING BELOW THIS LINE TO THE WORKER ⤵

You are a **WORKER** in the SlotSmart project's Supervisor-Worker AI Framework.

## 0. Initialization

Before doing anything else:

1. Read [`docs/SUPERVISOR/WORKER-FRAMEWORK.md`](../WORKER-FRAMEWORK.md). Initialize as WORKER per its instructions.
2. Read [`docs/SUPERVISOR/CURRENT-STATUS.md`](../CURRENT-STATUS.md). Skim — this is your project context.
3. Confirm with me ("ready to work on `{TASK-ID}` — here is my understanding: …"). **Do not start coding yet.**

## 1. Your task

**ID**: `{TASK-ID}` (e.g. `P1-T02`)
**Title**: `{TASK-TITLE}`
**Canonical spec**: [`docs/plan/{phase-folder}/{task-file}.md`](../../plan/{phase-folder}/{task-file}.md)

Read the canonical spec end-to-end. Treat it as the source of truth for **what** to build. This handoff adds **how** to operate within the framework.

## 2. What's already been done (state at handoff)

**Last commit**: `{git-sha-or-branch}`
**Open phase**: `Phase {N} — {Phase Name}`
**Tasks completed in this phase**: `{list IDs}` — see [`docs/SUPERVISOR/DELEGATION-TRACKER.md`](../DELEGATION-TRACKER.md).
**Recent CHANGELOG entries**: `{1-3 bullets summarizing what landed since the last handoff}`

**Your dependencies are met**: `{confirm explicitly: e.g. "P1-T01 is complete; the .NET solution exists at backend/SlotSmart.sln"}`

## 3. Architecture docs you must read first

In this order:

1. [`docs/plan/00-architecture/tech-stack.md`](../../plan/00-architecture/tech-stack.md)
2. [`docs/plan/00-architecture/solution-structure.md`](../../plan/00-architecture/solution-structure.md)
3. [`docs/plan/00-architecture/coding-standards.md`](../../plan/00-architecture/coding-standards.md)
4. [`docs/plan/00-architecture/api-conventions.md`](../../plan/00-architecture/api-conventions.md)  *(if your task adds endpoints)*
5. [`docs/plan/00-architecture/multi-tenancy-strategy.md`](../../plan/00-architecture/multi-tenancy-strategy.md)  *(if your task touches data or auth)*
6. [`docs/plan/00-architecture/domain-glossary.md`](../../plan/00-architecture/domain-glossary.md)  *(for any domain work)*

## 4. Constraints from this project

- Use the methodology skills already installed in `.cursor/skills/`:
  - `test-driven-development` for any new behavior
  - `systematic-debugging` for any failing test or unexpected output
  - `verification-before-completion` before claiming done
- Update `CHANGELOG.md` per [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/) — newest first, follow the user's project rules.
- Follow Clean Architecture layer rules — `Architecture.Tests` will fail the build if you don't.
- Multi-tenant safety: every query that returns club data must respect the tenant filter. (Phase 1 has no tenant data yet, but follow the convention from day one.)
- Ask before deviating from the architecture docs.

## 5. Phase-gate awareness

This task contributes to [`docs/SUPERVISOR/phase-gates/phase-{N}-gate.md`](../phase-gates/phase-{N}-gate.md).
The supervisor will run that gate before opening the next phase. Specifically, your work must satisfy these gate items:

- `{quote the §2 / §3 items relevant to this task}`

Design your tests so those gate commands will pass on a fresh clone.

## 6. Definition of Done (this handoff)

You are done when **all** of these are true:

- [ ] All Acceptance Criteria boxes in the canonical task spec are ticked, with evidence (test output, curl response, screenshot).
- [ ] `dotnet build -warnaserror` succeeds (or the equivalent for the frontend) — no new warnings.
- [ ] All new tests pass; you watched them fail before they passed (TDD).
- [ ] CHANGELOG.md has an entry under `Added` / `Changed` / `Fixed` as appropriate.
- [ ] You produced a completion report (see §7).

## 7. Completion report

Reply with the report template from `WORKER-FRAMEWORK.md` §"Completion Report Template", filled in for `{TASK-ID}`. Include:

- Files created/modified
- Test results (commands + output, not just "tests pass")
- Acceptance Criteria verification table
- Any deviations from the spec, with rationale
- Any open issues you discovered but didn't fix (so the SUPERVISOR can triage)

When the report is ready, the SUPERVISOR will validate it and either accept (mark `✅ COMPLETED` in the tracker) or send a remediation handoff.

---

## ⤴ COPY EVERYTHING ABOVE THIS LINE TO THE WORKER ⤴

---

## SUPERVISOR notes (do **not** copy to the worker)

- After dispatching, mark this task `🚧 IN PROGRESS` in `DELEGATION-TRACKER.md` with timestamp and worker session id (or "HYBRID" if executing yourself).
- Set a checkpoint timer — if no progress report in {expected duration based on size: S=2h, M=4h, L=8h}, ping the worker.
- When the report comes back, validate before accepting. Re-run at least one verification command yourself.
