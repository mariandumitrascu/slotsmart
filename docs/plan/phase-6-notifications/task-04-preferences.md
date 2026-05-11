# Task `P6-T04` — User Notification Preferences

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: M
> **Depends on**: P6-T01
> **Can run in parallel with**: P6-T02, P6-T03

---

## 1. Context

Users decide which categories they want and on which channels. Defaults are opinionated and per-role: Players get everything, Coaches get fewer (no per-booking emails), Parents get the same as their linked children (mirrored).

## 2. Goal

> A user can view and update their notification preferences. The dispatcher consults these before creating deliveries. Defaults are sensible and well-documented.

## 3. Scope

### In scope

- Entity `NotificationPreference`:
  - `Id`, `TenantId`, `MemberId`, `Category`, `Channel`, `Enabled` (bool).
  - Composite uniqueness `(MemberId, Category, Channel)`.
- Defaults:
  - Apply per role at member creation.
  - The default matrix lives in `NotificationPreferenceDefaults.cs` (in `Infrastructure/Notifications/`), keyed by `(Role, Category, Channel) → bool`.
- Endpoints:
  - `GET /api/v1/me/notification-preferences` → matrix or list with default fallbacks shown.
  - `PATCH /api/v1/me/notification-preferences` → array of upserts.
  - `POST /api/v1/me/notification-preferences:reset-to-defaults`.
- `INotificationPreferences` real implementation replacing the stub from P6-T01.
- Audit: `notifications.preferences.changed` (metadata = changed entries).

### Out of scope

- Per-tenant overrides of category visibility (e.g. "this club doesn't have waitlists") → V2.
- "Quiet hours" / digest preferences → V2.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)

## 5. Deliverables

- Entity + migration `AddNotificationPreferences` + unique index.
- Default seeding on member creation (hook into the existing handler).
- Endpoints + tests.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] New members are created with default preferences for their role.
- [ ] `GET /api/v1/me/notification-preferences` returns the effective matrix.
- [ ] Disabling `BookingConfirmed` × `Email` results in only `InApp` delivery for the next booking.
- [ ] `:reset-to-defaults` restores the role defaults.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- "Default" rows are not persisted unless the user changes them; the GET endpoint merges defaults with overrides. This keeps the table small.
- When a user changes role (P3-T03), don't reset their preferences; defaults only seed once at create time. Document.

## 9. Suggested execution outline

1. Entity + migration.
2. Defaults catalog.
3. Hook into member-created handler.
4. Endpoints + tests.
5. CHANGELOG.

## 10. Open questions / risks

- Question: do we expose preferences on member detail page or only "my settings"? **Decision (MVP)**: only "my settings" (own user). Admin overrides V2.
