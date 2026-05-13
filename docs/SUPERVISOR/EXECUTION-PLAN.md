# Execution Plan — SlotSmart MVP (Phases 1–5)

> **Owner**: SUPERVISOR session
> **Source**: [`docs/plan/`](../plan/) — phased plan written by the project owner
> **Mission scope**: Phases 1–5 (MVP / Release 1.0)
> **Started**: 2026-05-12
> **Last updated**: 2026-05-12

---

## 1. Mission

Deliver Release 1.0 (MVP) of SlotSmart by executing Phases 1–5 from `docs/plan/`. Each phase is a sequential gate; tasks within a phase can fan out where dependencies allow. The SUPERVISOR coordinates, validates, and gates phase advancement; WORKERS execute individual tasks.

---

## 2. Strategy

### 2.1 Execution mode per task

| Task profile | Mode | Rationale |
|---|---|---|
| Foundational scaffolding (P1) | Hybrid (Supervisor self-executes) | Single-file or single-purpose; fast feedback; sets conventions for all later workers |
| Cross-cutting infrastructure (e.g. tenancy filters, RBAC, outbox) | Pure Supervisor → delegate | Touches multiple layers; benefits from a fresh worker context |
| Backend feature task (P3–P5) | Pure Supervisor → delegate | Bounded scope; well-suited to a focused worker chat |
| Frontend feature task | Pure Supervisor → delegate | Often parallelizable with backend work |
| Hot fix during a phase | Hybrid | Speed > formality |

The SUPERVISOR may freely switch between modes per task. The choice is recorded in `DELEGATION-TRACKER.md` for traceability.

### 2.2 Phase gating

A phase is **closed** only when its phase-gate verification (see `phase-gates/`) passes:

1. All in-scope tasks marked `✅ COMPLETED` in `DELEGATION-TRACKER.md`.
2. The phase-gate test commands run clean (commands + expected outputs are checked in).
3. The phase's "Acceptance criteria for the whole phase" (from `docs/plan/phase-N-*/README.md`) are verified end-to-end.
4. `CHANGELOG.md` has entries for every task in the phase.
5. SUPERVISOR writes a phase-completion entry in `THINKING-LOG.md`.

Until all five conditions hold, **no work begins on the next phase.** Workers may continue to run in parallel within the open phase.

### 2.3 Sequencing summary

```
Phase 1 ──► Phase 2 ──► Phase 3 ──► Phase 4 ──► Phase 5
Foundation   Auth+        Club &      Training     Booking
             Tenancy      Members
  7 tasks     6 tasks      7 tasks     7 tasks      6 tasks   = 33 in-scope tasks
```

(Counts exclude Phase 6 & 7 which are out of mission scope but referenced where they affect MVP design choices.)

### 2.4 Parallelism within a phase

Once the unblocking task in a phase is done, remaining tasks fan out:

- **Phase 1**: After P1-T01, T04/T07 can start; T02/T05 next; T03 needs T02+T05; T06 needs T01+T05.
- **Phase 2**: T01 first, then T02 (long); T03/T04/T06 sequential after T02; T05 frontend can start once T02 has a token endpoint.
- **Phase 3**: T01 first; T02 unlocks T03/T04/T05 (parallelizable); T06/T07 frontend parallel.
- **Phase 4**: T01 unlocks T02/T04/T05; T03 needs T02; T06 frontend parallel; T07 needs T05+T06.
- **Phase 5**: T01 unlocks T02/T03/T05; T04 needs T01+P4-T05; T06 frontend last.

The SUPERVISOR may dispatch up to 2 workers in parallel inside a phase. Tighter parallelism is possible but adds coordination cost; only use it when work is genuinely independent (e.g., backend feature + frontend feature).

---

## 3. Phase plan with gate references

| # | Phase | Tasks | Gate doc | Success signal |
|---|---|---|---|---|
| 1 | Foundation | 7 | [`phase-gates/phase-1-gate.md`](./phase-gates/phase-1-gate.md) | `docker compose up` → React app shows `/api/v1/health` response from API reading Postgres; CI green |
| 2 | Auth & Tenancy | 6 | [`phase-gates/phase-2-gate.md`](./phase-gates/phase-2-gate.md) | Anonymous user signs up a club, signs in, hits `/api/v1/me`; cross-tenant access returns 404 |
| 3 | Club & Members | 7 | [`phase-gates/phase-3-gate.md`](./phase-gates/phase-3-gate.md) | Club Admin invites/edits/deactivates members; parent–child link works; coach profiles editable |
| 4 | Training | 7 | [`phase-gates/phase-4-gate.md`](./phase-gates/phase-4-gate.md) | Recurring training materializes; coach overlap rejected; attendance recorded; calendar UI |
| 5 | Booking | 6 | [`phase-gates/phase-5-gate.md`](./phase-gates/phase-5-gate.md) | Player books/cancels; waiting list auto-promotes; capacity enforced; outbox emits `BookingConfirmed` |

---

## 4. Risks & decisions tracked at the supervision level

| ID | Risk / Decision | Status | Resolution |
|---|---|---|---|
| R1 | .NET 10 SDK not GA — installed: 9.0.200 | **OPEN** | Plan §10 in P1-T01 specifies `global.json` with `rollForward: latestPreview`. Supervisor proposes: pin .NET 10 in `global.json` with latestPreview rollForward; if it does not resolve, fall back to .NET 9 and document deviation in DECISIONS-LOG. **Awaiting user input.** |
| R2 | EF Core 10 may also lag SDK availability | OPEN | Pin EF Core to latest stable preview matching chosen SDK |
| R3 | OpenIddict 6.x interaction with .NET 10 preview | OPEN | Verify in P2-T02 before committing |
| R4 | Time zones in P3-T01 (`ClubSettings.TimeZone`) and P4 (training scheduling) — TZDB on the host | OPEN | Use `TimeZoneInfo` with cross-platform fallback (NodaTime?). Decide in P3-T01 |
| R5 | Recurring training materialization horizon (12 weeks default per phase README, but how many?) | OPEN | Decide in P4-T02 with explicit ADR |
| R6 | Booking idempotency key strategy (header? request body?) | OPEN | Decide in P5-T01 with explicit ADR |
| D1 | Worker context isolation: each worker gets a fresh chat with a single handoff prompt | DECIDED | Reduces context pollution; matches framework intent |
| D2 | Phase gate is a checked-in markdown spec with executable commands | DECIDED | Anyone can re-verify a phase from a clean clone |

---

## 5. Working agreements (SUPERVISOR ↔ WORKER)

1. The SUPERVISOR provides the worker with **one** handoff prompt referencing exactly **one** task file from `docs/plan/`.
2. The worker reads `WORKER-FRAMEWORK.md` first, then the architecture docs, then the task file, then asks clarifying questions before coding.
3. The worker must produce a completion report using the template in `WORKER-FRAMEWORK.md` and update `CHANGELOG.md`.
4. The SUPERVISOR validates by running the verification commands listed in the task file and the phase-gate doc.
5. If verification fails, the SUPERVISOR sends the worker a remediation handoff prompt with the failure evidence — never accepts a pass without evidence.

---

## 6. Supervisor session checklist (per session)

```markdown
1. [ ] Read CURRENT-STATUS.md (state)
2. [ ] Read DELEGATION-TRACKER.md (active work)
3. [ ] Read THINKING-LOG.md last entry (recent decisions)
4. [ ] Identify the current phase + unblocked task(s)
5. [ ] If a worker is reporting completion: validate with phase-gate or task verification
6. [ ] If launching a new task: produce/update the handoff prompt under handoff-prompts/
7. [ ] If closing a phase: run the full phase-gate verification, log result, advance status
8. [ ] Update CURRENT-STATUS.md, DELEGATION-TRACKER.md, CHANGELOG.md before ending session
```

---

## 7. References

- Project plan: [`../../plan/README.md`](../../plan/README.md)
- Architecture: [`../../plan/00-architecture/`](../../plan/00-architecture/)
- Framework: [`./SUPERVISOR-FRAMEWORK.md`](./SUPERVISOR-FRAMEWORK.md), [`./WORKER-FRAMEWORK.md`](./WORKER-FRAMEWORK.md)
- Handoff prompts: [`./handoff-prompts/`](./handoff-prompts/)
- Phase gates: [`./phase-gates/`](./phase-gates/)

---

**Plan version**: 1.0
**Next review**: After Phase 1 gate result
