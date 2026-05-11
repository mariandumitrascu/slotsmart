# Task `P4-T05` — Attendance Tracking

> **Phase**: 4 — Training Management
> **Estimated size**: M
> **Depends on**: P4-T01
> **Can run in parallel with**: P4-T02, P4-T03, P4-T04

---

## 1. Context

Coaches need to mark who showed up. Attendance is per-(training, player) and feeds future analytics (coach performance, player engagement).

> Note: this task introduces the **expected** attendance list. Until Phase 5's booking exists, the expected list will simply be "no one"; the system supports manual addition of expected players. After Phase 5 lands, expected list = confirmed bookings.

## 2. Goal

> For any non-cancelled training, the coach can see the list of expected players and mark each as `Present | Absent | Excused | Late`. Marks are auditable and reversible until the training is marked `Completed`.

## 3. Scope

### In scope

- Aggregate `AttendanceRecord` in `Domain/Trainings/`:
  - `Id`, `TenantId`, `TrainingId`, `PlayerMemberId`, `Status` (`Expected | Present | Absent | Excused | Late`), `Note?`, `MarkedAt`, `MarkedByMemberId?`.
- Use cases:
  - `AddExpectedPlayers(TrainingId, MemberIds[])` (HeadCoach+ now; auto-called by booking handlers in Phase 5).
  - `RemoveExpectedPlayer(TrainingId, MemberId)`.
  - `MarkAttendance(TrainingId, MemberId, Status, Note?)` — only when training Status in (`Published`).
  - `CompleteTraining(TrainingId)` (Coach of the training or HeadCoach+) — flips Training to `Completed`; after this, attendance is read-only.
  - `GetTrainingAttendance(TrainingId)`.
- A nightly job auto-marks `Expected` players as `Absent` 24h after training end if not manually marked, AND flips the training to `Completed` if no one has touched it.
- Audit: `training.attendance.marked`, `training.completed`.

### Out of scope

- Player self-checkin → V2.
- Bulk import → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/domain-glossary.md`](../00-architecture/domain-glossary.md)

## 5. Deliverables

- `AttendanceRecord` aggregate + status enum.
- Migration `AddAttendanceRecords`.
- Application use cases + endpoints `AttendanceEndpoints.cs`.
- Quartz job `AttendanceAutoCloseJob`.
- Tests:
  - Cannot mark on a Cancelled or Completed training.
  - Re-marking the same player updates in place (not duplicate rows).
  - Auto-close after T+24h flips training to Completed.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] HeadCoach adds player A and B to a Published training; coach marks A=Present, B=Absent; the response shows both records.
- [ ] Coach can update B from Absent to Excused with a note.
- [ ] After `CompleteTraining`, marking returns `409`.
- [ ] After 24h with no marks, the nightly job marks Expected → Absent and Completes the training (verified by setting `IClock` to a test time).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Unique index on `(TrainingId, PlayerMemberId)` prevents duplicates.
- Only Coaches of the specific training (or HeadCoach+) can mark. Use `IResourceAuthorizationService` to check "is coach of this training".

## 9. Suggested execution outline

1. Aggregate + invariants + tests.
2. Migration.
3. Use cases + endpoints.
4. Auto-close job + tests.
5. CHANGELOG.

## 10. Open questions / risks

- Question: should `Late` count as "Present" for analytics? **Decision**: track as its own state; future analytics decides.
