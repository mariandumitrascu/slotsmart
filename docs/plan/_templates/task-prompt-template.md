# Task `P<phase>-T<task>` — <Short task title>

> **Phase**: <phase number and name>
> **Estimated size**: <S | M | L> (S ≈ ½ day, M ≈ 1–2 days, L ≈ 3–5 days)
> **Depends on**: <list of task IDs that must be done first; or "none">
> **Can run in parallel with**: <list of task IDs>

---

## 1. Context

<2–5 sentence summary of what the user / product is trying to do here. Reference the architecture docs that are most relevant (link them).>

## 2. Goal

> <One sentence, externally observable outcome. Example: "A coach can create a recurring weekly training and the system materializes the next 12 weeks of sessions, visible via `GET /api/v1/trainings`.">

## 3. Scope

### In scope

- <bullet>
- <bullet>

### Out of scope (handled by another task)

- <bullet — and link the task that owns it>

## 4. Inputs

- Existing code: <files / projects to extend>
- Architecture docs to read first:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
  - <others>
- External contracts / specs: <e.g. OpenAPI fragment, ERD>
- Config / env vars introduced: <name + purpose>

## 5. Deliverables

### Backend

- <file paths to create/modify>
- <endpoints to add: `POST /api/v1/...`>
- <migrations to add>

### Frontend

- <feature folder + screens>
- <generated client regeneration step if API changed>

### Tests

- <unit / integration / e2e expectations>

### Docs

- Update `CHANGELOG.md` per project rules.
- Update `docs/plan/<phase>/README.md` "Status" if applicable.
- Add/extend OpenAPI examples.

## 6. Acceptance Criteria (observable behavior)

- [ ] <Behavior 1, written so a non-developer can verify>
- [ ] <Behavior 2>
- [ ] <Behavior 3>

## 7. Definition of Done

- [ ] Code compiles with `TreatWarningsAsErrors=true` and no new warnings.
- [ ] All new unit/integration tests pass locally and in CI.
- [ ] `docker compose up` brings up the system and the new behavior works end-to-end.
- [ ] OpenAPI document is updated; the frontend client has been regenerated and committed.
- [ ] CHANGELOG.md updated under the right section (Keep a Changelog 1.1.0).
- [ ] No TODO / FIXME left without a tracked follow-up.
- [ ] PR description references this task ID and links the plan file.

## 8. Handoff notes / gotchas

- <Anything subtle the agent should know>
- <Pitfalls observed earlier>
- <Conventions to double-check>

## 9. Suggested execution outline

1. <step>
2. <step>
3. <step>

## 10. Open questions / risks

- <question — and who decides>
- <risk + mitigation>
