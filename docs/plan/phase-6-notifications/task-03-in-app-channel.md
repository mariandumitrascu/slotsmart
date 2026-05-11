# Task `P6-T03` — In-App Channel + API

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: M
> **Depends on**: P6-T01
> **Can run in parallel with**: P6-T02, P6-T04

---

## 1. Context

The in-app channel is the simplest dispatcher: a queued delivery flips to `Sent`, and a per-recipient unread counter ticks up. Real-time push to the UI is **not** required for MVP — the UI polls every 60s.

## 2. Goal

> Confirming a booking creates an `InApp` notification for the recipient; calling `GET /api/v1/me/notifications?unread=true` returns it; `POST /api/v1/notifications/{id}:read` marks it read; `GET /api/v1/me/notifications/summary` returns the unread count.

## 3. Scope

### In scope

- `InAppChannelDispatcher` — trivially flips `NotificationDelivery.Status = Sent` for `Channel=InApp`, sets `SentAt`. No external call.
- Frontend-facing endpoints (UI in P6-T07):
  - `GET /api/v1/me/notifications?cursor=...&limit=50&unread=true|false&category=...`
  - `GET /api/v1/me/notifications/summary` → `{ unreadCount }`.
  - `POST /api/v1/notifications/{id}:read` → `204`.
  - `POST /api/v1/me/notifications:read-all` → `204`.
- A small render contract for the UI: each notification has a normalized `title`, `body`, `link` (path within the app), `iconHint` (string token). These are computed by a server-side `NotificationRenderer` per category — same source of truth as the email templates' subject + key fields.
- SignalR / WebSocket → **out of scope** for MVP. Document polling interval (60s) and revisit in V2.

### Out of scope

- Real-time push → V2.
- Rich UI → P6-T07.

## 4. Inputs

- API conventions doc.

## 5. Deliverables

- `Infrastructure/Notifications/InApp/InAppChannelDispatcher.cs`.
- `Application/Features/Notifications/Renderers/NotificationRenderer.cs` shared by email subject + in-app title generation.
- `Api/Endpoints/NotificationsEndpoints.cs`.
- Tests:
  - Confirming a booking marks the in-app delivery `Sent` and bumps `unreadCount`.
  - Reading marks it read; subsequent `summary` reflects it.
  - Cursor pagination correct.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] After a booking confirm, `GET /api/v1/me/notifications/summary` returns `unreadCount` >= 1.
- [ ] The notification has a `link` that the frontend can route to.
- [ ] Marking as read is idempotent.
- [ ] Pagination by cursor works newest-first.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] OpenAPI + client regenerated.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Single source of truth: the `NotificationRenderer` produces both the `title`/`body` for in-app AND the data the email template uses. This avoids drift.

## 9. Suggested execution outline

1. Renderer.
2. Dispatcher.
3. Endpoints + tests.
4. CHANGELOG.

## 10. Open questions / risks

- none significant.
