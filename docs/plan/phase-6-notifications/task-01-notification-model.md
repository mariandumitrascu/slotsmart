# Task `P6-T01` — Notification Model + Dispatcher Pipeline

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: L
> **Depends on**: Phase 5 complete
> **Can run in parallel with**: nothing

---

## 1. Context

The notification system is a small platform of its own. We start with a model + pipeline that's channel-agnostic and audit-friendly. Channels (Email, InApp) plug in via interfaces; recipients × channels × preferences flow through a single dispatcher.

## 2. Goal

> Calling `INotificationService.SendAsync(NotificationRequest)` produces, for each recipient, the right set of channel messages based on preferences and recipient capabilities; each message is persisted, then dispatched to its channel; failures are retried.

## 3. Scope

### In scope

- Domain (or Infrastructure — pick **Infrastructure**, since this is infrastructural to the domain):
  - `NotificationCategory` enum (e.g. `BookingConfirmed | BookingCancelled | TrainingChanged | TrainingCancelled | TrainingReminder | WaitlistPromoted | InvitationSent | …`).
  - `NotificationChannel` enum (`Email | InApp`; later `Push | Sms | Messenger`).
  - `Notification` entity (one per category × actor × recipient): `Id`, `TenantId`, `Category`, `RecipientMemberId`, `SubjectType` (e.g. `Training | Booking`), `SubjectId`, `Data` (jsonb — the rendering context), `OccurredAt`, `RelatedNotificationId?` (for grouping).
  - `NotificationDelivery` entity (one per Notification × Channel): `Id`, `NotificationId`, `Channel`, `Status` (`Queued | Sent | Failed | Suppressed`), `Attempts`, `LastError?`, `SentAt?`, `ExternalId?` (provider id).
- Application:
  - `INotificationService.SendAsync(NotificationRequest)` where `NotificationRequest` has `Category`, `Recipients` (`MemberId[]`), `Subject`, `Data`.
  - `INotificationChannelDispatcher` interface (implemented by Email and InApp tasks).
  - The service:
    1. Loads recipient preferences (P6-T04 will fill these; default = opt-in for all categories on both channels).
    2. For each (recipient × channel) where opted in, persists `Notification` + `NotificationDelivery(Queued)`.
    3. Schedules an immediate dispatch via Quartz job `NotificationDispatchJob` (channel-agnostic; reads queued deliveries).
- Background dispatcher:
  - Picks up `Queued` deliveries (FOR UPDATE SKIP LOCKED).
  - Calls the channel dispatcher; on success `Sent`, on failure `Failed` with retry backoff (cap N=5 then dead-letter).
- Audit: notifications themselves serve as a log; do **not** double-write to audit.

### Out of scope

- Email rendering / SMTP → **P6-T02**.
- In-app UI → **P6-T03**.
- Preferences → **P6-T04**.
- Event handlers → **P6-T05**.
- Reminders → **P6-T06**.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)

## 5. Deliverables

- `Infrastructure/Notifications/` with all entities, EF configs, migration `AddNotifications`.
- `Application/Common/Abstractions/INotificationService.cs`, `INotificationChannelDispatcher.cs`, `INotificationPreferences.cs` (default impl returns "all on").
- `EfNotificationService.cs` implementation.
- `NotificationDispatchJob` (Quartz).
- Tests:
  - Send with two recipients × default prefs creates 4 deliveries (2 channels × 2 recipients).
  - Failed dispatch retries with backoff.
  - Suppressed (prefs say off) is recorded as `Suppressed`, not `Sent`.
- Admin endpoint `GET /api/v1/admin/notifications` (ClubAdmin) for ops visibility.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Calling `INotificationService.SendAsync` with category `BookingConfirmed` and 1 recipient and default preferences creates 1 `Notification` + 2 `NotificationDelivery` rows (`Email`, `InApp`).
- [ ] When channel dispatchers (P6-T02, P6-T03) are present, deliveries transition to `Sent`.
- [ ] Without dispatchers (this task in isolation), deliveries remain `Queued` and the test asserts they were created.
- [ ] Admin endpoint returns paginated, filterable list of recent notifications.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI updated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- The notification service must be callable from event handlers; keep it free of HttpContext dependencies.
- For reliability, the **creation of Notification + Delivery rows** happens inside the **outbox handler's transaction** (so it's exactly-once relative to the domain event). The actual **sending** is then asynchronous via the dispatch job — this gives us the "at-least-once + idempotent" combo we want.

## 9. Suggested execution outline

1. Entities + migration.
2. Service interface + default impl.
3. Dispatcher job + retry/backoff.
4. Tests.
5. Admin endpoint.
6. CHANGELOG.

## 10. Open questions / risks

- Question: do we group repeated category-per-day per recipient ("you have 3 new bookings")? **Decision (MVP)**: no grouping. V2.
