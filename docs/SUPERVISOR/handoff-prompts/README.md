# Handoff Prompts

> Ready-to-paste prompts that initialize a fresh worker chat session and give it everything it needs to execute one task from `docs/plan/` end-to-end.

---

## How to use

1. Open a **new chat session** in your AI IDE (Cursor, Windsurf, Cline, etc.).
2. Copy the contents of the relevant `P{n}-T{nn}-*.md` file in this folder.
3. Paste it as the first message to the worker.
4. The worker will:
   - Read `docs/SUPERVISOR/WORKER-FRAMEWORK.md` and initialize as WORKER.
   - Read the architecture docs and the linked task file.
   - Confirm understanding before coding.
   - Execute, test, and produce a completion report.
5. Bring the worker's completion report back to the SUPERVISOR session.
6. SUPERVISOR validates against the task's Acceptance Criteria and the relevant phase-gate items.

The same prompt works for delegating to a separate AI session OR for executing in **Hybrid Mode** in the supervisor session itself.

---

## Index (Phase 1 — Foundation)

| ID | Prompt | Status | Last validated |
|---|---|---|---|
| P1-T01 | [`P1-T01-solution-scaffolding.md`](./P1-T01-solution-scaffolding.md) | Ready | — |
| P1-T02 | (generate from `_template.md` when P1-T01 is in pending review) | Pending | — |
| P1-T03 | (generate after P1-T02) | Pending | — |
| P1-T04 | (generate after P1-T01) | Pending | — |
| P1-T05 | (generate after P1-T01) | Pending | — |
| P1-T06 | (generate after P1-T01 + P1-T05) | Pending | — |
| P1-T07 | (generate after P1-T01) | Pending | — |

Phase 2–5 prompts are generated **just-in-time** as upstream tasks finish. Generating all 38 up front would inevitably go stale (each prompt embeds "what's been done so far"). The `_template.md` makes generation a 2-minute exercise.

---

## Authoring rules (for the SUPERVISOR)

When writing a new handoff prompt:

1. Reference the canonical task file in `docs/plan/` — **don't duplicate it**.
2. Include a **fresh "what's been done" section** based on the latest `CURRENT-STATUS.md` + recent `CHANGELOG.md` entries. This is what the worker can't see otherwise.
3. Add a **"Dispatch metadata"** block at the top with **Suggested model**, **Why this model**, and **Suggested escalation** — placed **above** the `COPY TO WORKER` markers (this is for the human dispatching the prompt, not the worker). See "Model selection" below.
4. List the **exact files to read first** in priority order.
5. Include the **phase gate** items this task contributes to. The worker should design tests with the gate in mind.
6. Specify the **completion report format** the SUPERVISOR will accept (use the `WORKER-FRAMEWORK.md` template by default).
7. Save as `P{n}-T{nn}-{slug}.md`.

---

## Model selection

Each handoff suggests one of three tiers. Pick the cheapest model that can deliver acceptable quality; escalate only when the task profile demands it.

| Tier | Use for | Typical SlotSmart tasks |
|---|---|---|
| **Opus** (high-capability, expensive) | Cross-cutting architecture; novel algorithms; security-sensitive code; concurrency/correctness reasoning; ambiguous specs requiring judgment; ADR authoring | P2-T01 tenancy filters, P2-T02 OpenIddict + JWT, P4-T02 series materialization (DST, recurrence math), P4-T03 edit/cancel scope rules, P5-T01 booking aggregate + state machine, P5-T02 waitlist auto-promotion, P5-T05 outbox |
| **Sonnet** (balanced default) | Standard feature implementation when the spec is clear; CRUD + handlers + tests; most domain aggregates; non-trivial frontend features; refactors with clear targets | P1-T01 scaffolding, P1-T02 EF Core bootstrap, P1-T05 frontend scaffold, P1-T06 OpenAPI + client, P2-T03 RBAC, P2-T04 club signup, P2-T05 frontend auth, P3-T02 Member CRUD, P3-T05 parent–child, P4-T01 training aggregate, P4-T04 coach overlap, P4-T05 attendance, P4-T06 / T07 frontend, P5-T03 availability, P5-T04 booking↔attendance, P5-T06 frontend booking |
| **Auto** (cheap / cost-optimized) | Mechanical edits; config files; YAML; Docker / docker-compose; templating; doc updates; lint fixes; well-bounded tests | P1-T03 docker-compose, P1-T04 GitHub Actions CI, P1-T07 observability config (Serilog + OTel + Seq YAML/json), P2-T06 audit-log scaffolding, P3-T01 club settings (CRUD on a single entity), P3-T03 role mgmt, P3-T04 coach profile, P3-T06 / T07 frontend forms |

### Escalation rules

- If a worker on **Sonnet** asks more than two clarifying architectural questions, the SUPERVISOR re-dispatches to **Opus**.
- If a worker on **Auto** produces output that fails the gate's automated verification more than once, re-dispatch to **Sonnet**.
- If a task spec is updated mid-flight (scope change), the SUPERVISOR re-evaluates the model for the remaining work.

### When in doubt

Default to **Sonnet**. Escalate up to Opus only after observing actual difficulty; downshift to Auto only when the task is clearly mechanical.

---

## Template

See [`_template.md`](./_template.md).
