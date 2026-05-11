# Phase 5 — Booking System

> Release: **1.0 (MVP)** · Depends on: Phase 4 complete · Unlocks: a usable MVP.

## Goal

Let Players (and Parents on behalf of Children) **book**, **cancel**, and **join waiting lists** for trainings. Availability is enforced; the system never over-books; the waiting list moves with confirmed cancellations.

## Outcomes

- A player can see the trainings available to them (filtered by visibility rules) and book/cancel with one click.
- A parent can book on behalf of a child.
- When a training is full, booking adds to the waiting list (if enabled).
- Cancelling a confirmed booking auto-promotes the head of the waiting list (configurable).
- The booking window and cancellation window from `ClubSettings` are enforced.
- Bookings are linked to attendance: confirmed bookings become Expected attendees.
- Idempotent booking endpoints support retries.
- Frontend has a booking UI on the calendar and a "My bookings" page.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P5-T01 | [Booking aggregate + state machine](./task-01-booking-aggregate.md) | L | P4 done |
| P5-T02 | [Waiting list + auto-promotion](./task-02-waiting-list.md) | M | P5-T01 |
| P5-T03 | [Availability & booking-window enforcement](./task-03-availability.md) | M | P5-T01 |
| P5-T04 | [Booking ↔ Attendance integration](./task-04-booking-attendance.md) | S | P5-T01, P4-T05 |
| P5-T05 | [Outbox + domain events for bookings](./task-05-outbox.md) | M | P5-T01 |
| P5-T06 | [Frontend: book/cancel on calendar + "My bookings"](./task-06-frontend-booking.md) | L | P5-T01..T04 |

## Acceptance criteria for the whole phase

- [ ] A Player can book a published, in-window training; capacity is enforced; second attempt returns `409`.
- [ ] A Parent can book on behalf of their linked Child (minor); without the link → `403`.
- [ ] Booking a full training with waitlist enabled adds the player to the waiting list; with waitlist disabled returns `422`.
- [ ] Cancelling a confirmed booking auto-promotes the head of the waiting list (if `autoPromote=true`).
- [ ] Cancellation past `CancellationWindowHours` is allowed but flagged with `lateCancel: true` for future analytics.
- [ ] Confirmed bookings appear as `Expected` attendees in attendance UI.
- [ ] An outbox-dispatched event `BookingConfirmed` is observable via test consumer (will drive notifications in Phase 6).
- [ ] Frontend lets a Player browse, book, and cancel; "My bookings" lists upcoming + past.
