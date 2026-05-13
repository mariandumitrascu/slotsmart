# Phase Gates

> Verification specs that gate advancement from one phase to the next.
> A phase is **closed** only when its gate passes per the rules in [`../EXECUTION-PLAN.md`](../EXECUTION-PLAN.md) §2.2.

---

## Index

| Gate | From → To | Doc | Status |
|---|---|---|---|
| G1 | Phase 1 → Phase 2 | [`phase-1-gate.md`](./phase-1-gate.md) | ⏳ Not yet run |
| G2 | Phase 2 → Phase 3 | [`phase-2-gate.md`](./phase-2-gate.md) | ⏳ Not yet run |
| G3 | Phase 3 → Phase 4 | [`phase-3-gate.md`](./phase-3-gate.md) | ⏳ Not yet run |
| G4 | Phase 4 → Phase 5 | [`phase-4-gate.md`](./phase-4-gate.md) | ⏳ Not yet run |
| G5 | Phase 5 → Phase 6 (MVP DONE) | [`phase-5-gate.md`](./phase-5-gate.md) | ⏳ Not yet run |

---

## Anatomy of a phase gate

Each gate doc contains:

1. **Pre-conditions** — task-completion checklist (mirrors the phase's tasks).
2. **Automated verification** — concrete shell commands a supervisor can copy-paste, with expected outputs / exit codes.
3. **Manual verification** — observable behaviors a supervisor (or product owner) checks in the running app.
4. **Promotion checklist** — the boxes the SUPERVISOR ticks before opening the next phase.
5. **Run log** — a table the SUPERVISOR appends to each time the gate is evaluated (date, result, evidence link, notes).

If a gate fails, the SUPERVISOR creates a remediation delegation pointing at the failing item — phase remains open.

---

## Why gates are checked-in markdown (not a CI job)

- Gates evolve with the project; a markdown spec is easy to amend and review in PRs.
- The commands are also runnable in CI later; markdown is a superset.
- Anyone can verify a phase from a fresh clone by reading the gate doc and running the listed commands.

A future enhancement could promote gate commands into a `tools/phase-gate.sh` script. For now the markdown is the source of truth.
