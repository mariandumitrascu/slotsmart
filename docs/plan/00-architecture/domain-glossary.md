# Domain Glossary

> The vocabulary used everywhere in code, docs, and UI. If you need a new term, add it here first.

## Tenancy & identity

- **Tenant** — A tennis club operating on the platform. The unit of data isolation. Has a `Slug` and a `Status`.
- **Platform Admin** — An employee of SlotSmart with cross-tenant access. Lives outside the tenant model.
- **User** — A human with credentials to sign in. Belongs to exactly one tenant (MVP).
- **Member** — A user's profile inside a club. One user → one member per tenant. Carries the **Roles** the user holds in this club.

## People & roles

- **Player** — A member who participates in trainings. Can be an adult or a child.
- **Parent** — A member who is the guardian of one or more Players. Manages bookings on the child's behalf.
- **Coach** — A member who can lead trainings. Can also be a Player at the same club.
- **Head Coach** — A coach with additional privileges: assign other coaches, edit schedules, configure capacity defaults.
- **Club Admin** — A member with full club administration rights, except billing (deferred).
- **Member Relation** — A directed link between two Members. Used primarily for `ParentOf` / `GuardianOf`. Future relation types possible.

> A single user can hold **multiple roles** at the same club (e.g. Coach + Player). Role checks must be inclusive (`hasAnyRole`), not exclusive.

## Scheduling

- **Training** — A scheduled session, group or individual. Has a start time, duration, location, coach(es), capacity, and a status.
- **Training Series** — A recurring definition that materializes into multiple Trainings (e.g. "Mondays and Wednesdays, 18:00–19:30, for 12 weeks"). Editing a series can fan out to its trainings.
- **Capacity** — Maximum number of Players that can be booked. `0` means uncapped (rare; allowed only with explicit flag).
- **Attendance** — A record per Player per Training. States: `Expected | Present | Absent | Excused | Late`.

## Booking

- **Booking** — A Player's reserved spot in a Training. States: `Confirmed | WaitingList | Cancelled`.
- **Waiting List** — Ordered queue of Bookings in `WaitingList` state for a Training. When a Confirmed Booking is cancelled, the head of the waiting list is auto-promoted (configurable).
- **Booking Window** — The configured `[opensAt, closesAt]` interval during which a Training is bookable. Defaults to "from publish until start time minus N hours."
- **Cancellation Window** — The latest moment at which a Player can cancel without penalty. Defaults to T-24h.

## Communication

- **Notification** — A message delivered to a User via one or more channels. Channels MVP: `Email`, `InApp`. Channels later: `Push`, `SMS`, `Messenger`.
- **Reminder** — A scheduled notification fired at a relative time before an event (e.g. T-24h before a training).
- **Notification Preference** — Per-user opt-in / opt-out per category + channel.

## Cross-cutting concepts

- **Aggregate** — DDD term. The consistency boundary. Examples: `Member`, `Training`, `Booking`.
- **Domain Event** — A past-tense fact (`BookingConfirmed`, `TrainingCancelled`). Dispatched via the outbox after the transaction commits.
- **Outbox** — Reliable event dispatch pattern. Events are persisted in the same transaction as state changes and then relayed by a background worker.
- **Audit Log** — Append-only record of who did what when. Mandatory for membership changes and booking actions.

## Identifiers & time

- All public identifiers are **UUIDv7** (`Guid` in C#).
- All times are stored as `timestamp with time zone` UTC. UI converts to the **tenant default time zone** (a property on `Tenant`) and the **user's preferred time zone** if set.
- Dates without time (e.g. birthdate) are stored as `date`.

## Common invariants (MVP)

- A User belongs to exactly one Tenant.
- A Member must have at least one Role.
- A Player who is a child (`IsMinor=true`) cannot self-book; their Parent must.
- A Coach cannot be assigned to two overlapping Trainings.
- A Training's Bookings (Confirmed) count never exceeds Capacity. The waiting list is uncapped by default but configurable.
- Bookings are unique per `(TrainingId, PlayerId)` regardless of state, except `Cancelled` which doesn't block re-booking.
