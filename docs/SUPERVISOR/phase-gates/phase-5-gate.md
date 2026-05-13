# Phase Gate G5 — Booking → MVP DONE

> **Gate purpose**: Verify the booking system completes the MVP. After this gate, SlotSmart is ready for a pilot club.
> **Source phase plan**: [`../../plan/phase-5-booking/README.md`](../../plan/phase-5-booking/README.md)
> **Status**: ⏳ Not yet run

---

## 1. Pre-conditions

- [ ] **P5-T01** Booking aggregate + state machine
- [ ] **P5-T02** Waiting list + auto-promotion
- [ ] **P5-T03** Availability & booking-window enforcement
- [ ] **P5-T04** Booking ↔ Attendance integration
- [ ] **P5-T05** Outbox + domain events for bookings
- [ ] **P5-T06** Frontend: book/cancel on calendar + "My bookings"

---

## 2. Automated verification

(Use a Player token `$PLAYER_TOKEN`, a Parent token `$PARENT_TOKEN` linked to a Child, and a `$TRAINING_ID` from Phase 4 — capacity 1, waitlist enabled.)

### 2.1 Player books an in-window training

```bash
curl -fsS -X POST http://localhost:5080/api/v1/bookings \
  -H "Authorization: Bearer $PLAYER_TOKEN" \
  -H 'Content-Type: application/json' \
  -H "Idempotency-Key: $(uuidgen)" \
  -d "{\"trainingId\":\"$TRAINING_ID\"}" | jq .
```

**Expected**: 201; status `Confirmed`.

### 2.2 Second booking on a full training → 409

```bash
curl -s -o /tmp/body -w '%{http_code}\n' -X POST http://localhost:5080/api/v1/bookings \
  -H "Authorization: Bearer $PLAYER_TOKEN" \
  -H "Idempotency-Key: $(uuidgen)" \
  -d "{\"trainingId\":\"$TRAINING_ID\"}"
```

**Expected**: `409` (already booked). A different player on a full training should also get `409` if waitlist is **disabled**, or `201 status: Waitlisted` if waitlist is enabled.

### 2.3 Parent books on behalf of linked child

```bash
curl -fsS -X POST http://localhost:5080/api/v1/bookings \
  -H "Authorization: Bearer $PARENT_TOKEN" \
  -H "Idempotency-Key: $(uuidgen)" \
  -d "{\"trainingId\":\"$ANOTHER_TRAINING_ID\",\"forMemberId\":\"$CHILD_ID\"}" | jq .
```

**Expected**: 201. Same call without the parent–child link → `403`.

### 2.4 Idempotency key returns the same result

```bash
KEY=$(uuidgen)
curl -fsS -X POST http://localhost:5080/api/v1/bookings \
  -H "Authorization: Bearer $PLAYER_TOKEN" -H "Idempotency-Key: $KEY" \
  -d "{\"trainingId\":\"$T2\"}" | jq -r .id
# Replay
curl -fsS -X POST http://localhost:5080/api/v1/bookings \
  -H "Authorization: Bearer $PLAYER_TOKEN" -H "Idempotency-Key: $KEY" \
  -d "{\"trainingId\":\"$T2\"}" | jq -r .id
```

**Expected**: same booking id both times; second call does not create a duplicate row.

### 2.5 Auto-promotion from waitlist on cancel

```bash
# Player A confirmed, Player B waitlisted on a capacity-1 training.
curl -fsS -X DELETE http://localhost:5080/api/v1/bookings/{aBookingId} \
  -H "Authorization: Bearer $TOKEN_A"

# Player B's booking transitions Waitlisted → Confirmed
curl -fsS http://localhost:5080/api/v1/bookings/{bBookingId} \
  -H "Authorization: Bearer $TOKEN_B" | jq .status
```

**Expected**: B's status is `Confirmed`.

### 2.6 Late cancel flagged

```bash
# Cancel within CancellationWindowHours
curl -i -X DELETE http://localhost:5080/api/v1/bookings/{id} -H "Authorization: Bearer $TOKEN"
```

**Expected**: 200; response body includes `lateCancel: true` (per phase acceptance).

### 2.7 Confirmed booking shows up as Expected attendee

```bash
curl -fsS http://localhost:5080/api/v1/trainings/{trainingId}/attendance \
  -H "Authorization: Bearer $TOKEN" | jq '.expected[].memberId'
```

**Expected**: includes the booked member's id.

### 2.8 Outbox dispatches `BookingConfirmed`

```bash
# Create a booking, then poll the test outbox consumer / inspect outbox table
psql "$DB_URL" -c "select event_type, processed_at from outbox where event_type='BookingConfirmed' order by id desc limit 1;"
```

**Expected**: row exists with `processed_at` set within a few seconds.

### 2.9 Integration tests

```bash
cd backend && dotnet test --filter "Category=Phase5|FullyQualifiedName~Booking"
```

**Expected**: All pass.

---

## 3. Manual verification (frontend)

- [ ] Player browses the calendar and books a training in one click.
- [ ] When training is full, the button reads "Join waitlist".
- [ ] "My bookings" page lists upcoming + past bookings; cancel works.
- [ ] After a Player cancels, the next waitlisted user sees their booking flip to Confirmed on next refresh.

---

## 4. Promotion checklist (= **MVP DONE**)

- [ ] §1 tasks `✅ COMPLETED`
- [ ] §2 commands clean
- [ ] §3 manual checks confirmed
- [ ] CHANGELOG entries for all P5 tasks
- [ ] DECISIONS-LOG ADR for idempotency-key strategy (R6)
- [ ] THINKING-LOG: "Phase 5 closed — MVP READY"
- [ ] Tag `v1.0.0-mvp` in git
- [ ] Release notes drafted

---

## 5. Run log

| Date | Result | Evidence | Notes |
|---|---|---|---|
| _empty_ | — | — | — |

---

**Gate version**: 1.0
