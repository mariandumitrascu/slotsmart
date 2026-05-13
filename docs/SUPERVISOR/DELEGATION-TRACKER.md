# Delegation Tracker
## SlotSmart - Task Delegation & Progress Tracking

**Last Updated**: 2026-05-12
**Active Delegations**: 0
**Completed This Week**: 0
**In-scope tasks (P1–P5)**: 33

> Mission: execute Phases 1–5 (MVP). Source plan: [`../plan/`](../plan/). Strategy: [`./EXECUTION-PLAN.md`](./EXECUTION-PLAN.md). Gates: [`./phase-gates/`](./phase-gates/). Handoffs: [`./handoff-prompts/`](./handoff-prompts/).

---

## 🟢 ACTIVE DELEGATIONS

**No active delegations.** Ready to dispatch `P1-T01` (handoff prompt prepared).

| Task | Worker | Mode | Started | Last update | Phase gate | Status |
|---|---|---|---|---|---|---|
| _empty_ | | | | | | |

---

## 🟡 PENDING REVIEW

**No tasks pending review.**

| Task | Worker | Reported | Validation status |
|---|---|---|---|
| _empty_ | | | |

---

## ✅ RECENTLY COMPLETED

**No completed tasks yet.**

| Task | Worker | Completed | Phase gate progressed |
|---|---|---|---|
| _empty_ | | | |

---

## ❌ CANCELLED / ON HOLD

**None.**

---

## 📋 FULL TASK INVENTORY (Phases 1–5, 33 tasks)

Status legend: ⏳ queued · 📦 ready (handoff prompt prepared) · 🚧 in progress · 🟡 review · ✅ done · ❌ blocked

> **Model column** key: **Opus** = high-capability (architecture/correctness/security) · **Sonnet** = balanced default · **Auto** = cheap/mechanical. See [`./handoff-prompts/README.md`](./handoff-prompts/README.md#model-selection) for selection rules and escalation policy.

### Phase 1 — Foundation (7 tasks · gate G1)

| ID | Task | Size | Dep | Model | Status | Handoff prompt | Worker / mode | Notes |
|---|---|---|---|---|---|---|---|---|
| P1-T01 | Repository & solution scaffolding | M | — | Sonnet | 📦 ready | [P1-T01](./handoff-prompts/P1-T01-solution-scaffolding.md) | _unassigned_ | Sets conventions; escalate to Opus if .NET 10 preview unavailable (ADR-006) |
| P1-T02 | Database, EF Core, migrations bootstrap | M | T01 | Sonnet | ⏳ queued | _generate after T01_ | — | |
| P1-T03 | Docker & docker-compose dev env | M | T01,T02 | Auto | ⏳ queued | _generate after T02_ | — | Mostly YAML/Dockerfiles |
| P1-T04 | CI pipeline (GitHub Actions) | S | T01 | Auto | ⏳ queued | _generate after T01_ | — | Mostly YAML; runs parallel with T02 |
| P1-T05 | Frontend scaffolding (Vite + MUI + Router) | M | T01 | Sonnet | ⏳ queued | _generate after T01_ | — | Sets frontend conventions |
| P1-T06 | OpenAPI + generated TS client + health endpoint | M | T01,T05 | Sonnet | ⏳ queued | _generate after T05_ | — | |
| P1-T07 | Observability baseline (Serilog + OTel + Seq) | S | T01 | Auto | ⏳ queued | _generate after T01_ | — | Mostly config |

### Phase 2 — Auth & Multi-Tenancy (6 tasks · gate G2)

| ID | Task | Size | Dep | Model | Status | Handoff prompt | Worker | Notes |
|---|---|---|---|---|---|---|---|---|
| P2-T01 | Tenant resolution & EF query filters | M | P1✓ | **Opus** | ⏳ queued | — | — | Cross-cutting; correctness-critical |
| P2-T02 | Authentication: OpenIddict + JWT + refresh | L | T01 | **Opus** | ⏳ queued | — | — | Security-sensitive; R3 OpenIddict on .NET 10 |
| P2-T03 | RBAC end-to-end | M | T02 | Sonnet | ⏳ queued | — | — | |
| P2-T04 | Club signup & first admin onboarding | M | T02,T03 | Sonnet | ⏳ queued | — | — | |
| P2-T05 | Frontend auth flow | L | T02 | Sonnet | ⏳ queued | — | — | Starts once T02 has token endpoint |
| P2-T06 | Audit log infrastructure | S | T02 | Auto | ⏳ queued | — | — | Schema + interceptor |

### Phase 3 — Club & Members (7 tasks · gate G3)

| ID | Task | Size | Dep | Model | Status | Notes |
|---|---|---|---|---|---|---|
| P3-T01 | Club settings domain & endpoints | M | P2✓ | Sonnet | ⏳ queued | Decide TZ library (R4) — escalate to Opus if NodaTime adoption is in scope |
| P3-T02 | Member aggregate + CRUD + invitations | L | T01 | Sonnet | ⏳ queued | |
| P3-T03 | Role management on Member | M | T02 | Auto | ⏳ queued | |
| P3-T04 | Coach profile extension | M | T02 | Auto | ⏳ queued | |
| P3-T05 | Parent–Child relations (MemberRelation) | M | T02 | Sonnet | ⏳ queued | |
| P3-T06 | Frontend: club settings + member directory + invitations | L | T01,T02 | Sonnet | ⏳ queued | |
| P3-T07 | Frontend: coach profiles + parent/child mgmt | M | T04,T05,T06 | Auto | ⏳ queued | Forms over established CRUD |

### Phase 4 — Training (7 tasks · gate G4)

| ID | Task | Size | Dep | Model | Status | Notes |
|---|---|---|---|---|---|---|
| P4-T01 | Training aggregate (one-off) | M | P3✓ | Sonnet | ⏳ queued | |
| P4-T02 | TrainingSeries + materialization | L | T01 | **Opus** | ⏳ queued | Recurrence/DST math; horizon decision (R5) |
| P4-T03 | Edit/cancel single occurrence vs series | M | T02 | **Opus** | ⏳ queued | Semantic complexity (this/thisAndFuture/all) |
| P4-T04 | Coach overlap detection | M | T01 | Sonnet | ⏳ queued | |
| P4-T05 | Attendance tracking | M | T01 | Sonnet | ⏳ queued | |
| P4-T06 | Frontend: training calendar + create/edit | L | T01,T02,T03 | Sonnet | ⏳ queued | |
| P4-T07 | Frontend: attendance UI | M | T05,T06 | Sonnet | ⏳ queued | |

### Phase 5 — Booking (6 tasks · gate G5 = MVP DONE)

| ID | Task | Size | Dep | Model | Status | Notes |
|---|---|---|---|---|---|---|
| P5-T01 | Booking aggregate + state machine | L | P4✓ | **Opus** | ⏳ queued | Concurrency + idempotency (R6) |
| P5-T02 | Waiting list + auto-promotion | M | T01 | **Opus** | ⏳ queued | Concurrency-sensitive |
| P5-T03 | Availability & booking-window enforcement | M | T01 | Sonnet | ⏳ queued | |
| P5-T04 | Booking ↔ Attendance integration | S | T01,P4-T05 | Sonnet | ⏳ queued | |
| P5-T05 | Outbox + domain events for bookings | M | T01 | **Opus** | ⏳ queued | Transactional reliability; foundation for Phase 6 |
| P5-T06 | Frontend: book/cancel + "My bookings" | L | T01..T04 | Sonnet | ⏳ queued | |

---

## 📊 STATISTICS

### Phase 1–5 mission progress
- Tasks ready to dispatch: **1** (P1-T01)
- Tasks queued (waiting on dependencies): **32**
- Tasks in progress: 0
- Tasks completed: 0
- Phase gates passed: 0 / 5

### Model distribution (initial recommendations, may shift on escalation)
- **Opus** (high-capability): **7** — P2-T01/T02, P4-T02/T03, P5-T01/T02/T05
- **Sonnet** (balanced default): **19**
- **Auto** (cheap/mechanical): **7** — P1-T03/T04/T07, P2-T06, P3-T03/T04/T07
- Rationale per task in the tables above; selection rules in [`./handoff-prompts/README.md`](./handoff-prompts/README.md#model-selection)

### This Week
- Started: 0 · Completed: 0 · Blocked: 0

### All Time
- Delegated: 0 · Completed: 0 · Success rate: N/A

---

## 📝 DELEGATION OPERATING MODEL

The detailed authoring rules and worker contract live in:

- [`./EXECUTION-PLAN.md`](./EXECUTION-PLAN.md) — strategy and sequencing
- [`./handoff-prompts/README.md`](./handoff-prompts/README.md) — how to author/use handoff prompts
- [`./handoff-prompts/_template.md`](./handoff-prompts/_template.md) — canonical handoff shape
- [`./phase-gates/README.md`](./phase-gates/README.md) — verification approach
- [`./WORKER-FRAMEWORK.md`](./WORKER-FRAMEWORK.md) — worker initialization protocol

(The legacy generic templates that previously lived in this file have moved into the handoff-prompts directory.)

### Delegation lifecycle

```
1. SUPERVISOR generates handoff prompt from _template.md → status 📦 ready
2. SUPERVISOR dispatches to a worker chat (or self-executes in Hybrid mode) → 🚧 in progress
3. Worker submits completion report → 🟡 review
4. SUPERVISOR validates (re-runs verification) → ✅ done OR send remediation handoff (back to 🚧)
5. When all phase tasks ✅, SUPERVISOR runs the phase gate
6. If gate passes → next phase opens; if not → fix and re-run gate
```

---

## 🔄 PROCESS NOTES

### How to update this tracker

When a task changes state, **update the table for that task only** (not the whole file). Append a "Recently Completed" row when promoting from review → done. Bump `Last Updated` at the top of the file.

### Stale-task detection

If any task is `🚧 in progress` for more than its size budget without a worker progress update, flag it `❌ blocked` and write a `THINKING-LOG.md` entry with the suspected cause.

---

**Tracker Version**: 2.0
**Last Updated**: 2026-05-12
**Next Review**: After P1-T01 completion report
