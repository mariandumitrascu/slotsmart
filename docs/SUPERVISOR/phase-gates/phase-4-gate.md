# Phase Gate G4 — Training → Booking

> **Gate purpose**: Verify scheduling — one-off + recurring trainings, attendance, coach overlap detection — works end-to-end.
> **Source phase plan**: [`../../plan/phase-4-training/README.md`](../../plan/phase-4-training/README.md)
> **Status**: ⏳ Not yet run

---

## 1. Pre-conditions

- [ ] **P4-T01** Training aggregate (one-off)
- [ ] **P4-T02** TrainingSeries + materialization
- [ ] **P4-T03** Edit/cancel single occurrence vs series
- [ ] **P4-T04** Coach overlap detection
- [ ] **P4-T05** Attendance tracking
- [ ] **P4-T06** Frontend: training calendar + create/edit
- [ ] **P4-T07** Frontend: attendance UI

---

## 2. Automated verification

### 2.1 One-off training

```bash
curl -fsS -X POST http://localhost:5080/api/v1/trainings \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{
    "kind":"OneOff",
    "title":"U12 fitness",
    "startUtc":"2026-06-01T16:00:00Z",
    "endUtc":"2026-06-01T17:30:00Z",
    "coachIds":["{coachId}"],
    "capacity":8,
    "location":"Court 1"
  }' | jq .
```

**Expected**: 201; body returns the new training id.

### 2.2 Recurring series materializes 24 sessions (Mon+Wed for 12 weeks)

```bash
curl -fsS -X POST http://localhost:5080/api/v1/training-series \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{
    "title":"U12 group",
    "rrule":"FREQ=WEEKLY;BYDAY=MO,WE;COUNT=24",
    "startTimeOfDay":"18:00",
    "durationMinutes":90,
    "coachIds":["{coachId}"],
    "capacity":8,
    "location":"Court 1"
  }' | jq .

# Then list trainings between dates and assert count == 24
curl -fsS "http://localhost:5080/api/v1/trainings?from=2026-06-01&to=2026-09-01" \
  -H "Authorization: Bearer $TOKEN" | jq '.items | length'
```

**Expected**: `24`. (Phase acceptance: "Monday & Wednesday … for 12 weeks → 24 Training rows".)

### 2.3 Edit single occurrence does not affect siblings

```bash
# Move one occurrence
curl -fsS -X PUT http://localhost:5080/api/v1/trainings/{occurrenceId} \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"startUtc":"2026-06-03T19:00:00Z","endUtc":"2026-06-03T20:30:00Z","scope":"this"}'

# Verify other occurrences untouched
curl -fsS "http://localhost:5080/api/v1/trainings?from=2026-06-01&to=2026-06-30" \
  -H "Authorization: Bearer $TOKEN" | jq '.items | map(.startUtc)'
```

**Expected**: only the moved occurrence has the new time.

### 2.4 "This and future" edit updates the right occurrences

```bash
curl -fsS -X PUT http://localhost:5080/api/v1/trainings/{occurrenceId} \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"location":"Court 2","scope":"thisAndFuture"}'
```

**Expected**: occurrence + every later occurrence in the series has location=`Court 2`; earlier ones unchanged.

### 2.5 Coach overlap detection returns 422

```bash
curl -s -o /tmp/body -w '%{http_code}\n' -X POST http://localhost:5080/api/v1/trainings \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{
    "kind":"OneOff",
    "title":"Overlap test",
    "startUtc":"2026-06-01T16:30:00Z",
    "endUtc":"2026-06-01T17:00:00Z",
    "coachIds":["{coachId}"],
    "capacity":4
  }'
cat /tmp/body | jq .
```

**Expected**: HTTP `422` with documented error code (e.g. `coach_overlap`) and a list of conflicting training IDs.

### 2.6 Attendance lifecycle

```bash
# Mark Present / Absent / Excused / Late for an expected player
curl -fsS -X PUT http://localhost:5080/api/v1/trainings/{trainingId}/attendance \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"records":[{"memberId":"{m1}","state":"Present"},{"memberId":"{m2}","state":"Late"}]}'

# Read back
curl -fsS http://localhost:5080/api/v1/trainings/{trainingId}/attendance -H "Authorization: Bearer $TOKEN" | jq .
```

**Expected**: states persist exactly. Audit log captures `AttendanceRecorded`.

### 2.7 Integration & domain tests

```bash
cd backend && dotnet test --filter "Category=Phase4|FullyQualifiedName~Training|FullyQualifiedName~Attendance"
```

**Expected**: All pass.

---

## 3. Manual verification (frontend)

- [ ] Calendar week view renders trainings for the active club; click → detail panel.
- [ ] Head Coach can create a new training from the "+" button on the calendar.
- [ ] Editing a series prompts for scope (`this | thisAndFuture | all`).
- [ ] Attendance UI lists expected attendees and lets the coach mark each state.

---

## 4. Promotion checklist

- [ ] §1 tasks `✅ COMPLETED`
- [ ] §2 commands clean
- [ ] §3 manual checks confirmed
- [ ] CHANGELOG entries for all P4 tasks
- [ ] DECISIONS-LOG ADR for materialization horizon (R5 from EXECUTION-PLAN)
- [ ] THINKING-LOG: "Phase 4 closed"

---

## 5. Run log

| Date | Result | Evidence | Notes |
|---|---|---|---|
| _empty_ | — | — | — |

---

**Gate version**: 1.0
