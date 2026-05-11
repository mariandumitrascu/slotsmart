# Task `P6-T07` — Frontend: Notifications Bell + Page + Preferences

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: L
> **Depends on**: P6-T03, P6-T04
> **Can run in parallel with**: P6-T05, P6-T06

---

## 1. Context

The user-facing surface for notifications: a bell icon in the header with unread count, a dropdown of recent notifications, a full notifications page, and a notification-preferences section in user settings.

## 2. Goal

> A user sees a bell with an unread badge; clicking it shows the latest 10 notifications with deep links; a "see all" link opens a full page with filters and infinite scroll; a "Settings → Notifications" section lets the user toggle categories × channels.

## 3. Scope

### In scope

- `features/notifications/`:
  - `components/NotificationBell.tsx` — header item; polls summary every 60s; shows badge.
  - `components/NotificationDropdown.tsx` — recent 10 with mark-as-read; "See all" link.
  - `pages/NotificationsPage.tsx` (`/app/notifications`) — infinite scroll, filter by category, unread toggle.
  - `pages/NotificationPreferencesPage.tsx` (`/app/settings/notifications`) — matrix UI (categories × channels) with reset-to-defaults.
  - `api/notifications.api.ts`.
- The dropdown deep-links each item to the appropriate route (booking detail, training detail, etc.). The renderer's `link` field drives this.
- All strings i18n'd.
- Polling: 60s. When the dropdown opens, refresh immediately. Cancel polling when the user is signed out or the tab is hidden (`document.visibilityState`).

### Out of scope

- Real-time push → V2.
- Mute-per-thread / mute-per-training → V2.

## 4. Inputs

- API contracts from P6-T03, P6-T04.

## 5. Deliverables

- All listed files.
- E2E test `frontend/tests/notifications.spec.ts`: trigger a booking confirm, verify the bell badge increments, open the dropdown, click into the booking, verify it's marked read.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Bell shows unread count; updates within 60s of a new notification.
- [ ] Clicking a notification routes to the right page AND marks it read.
- [ ] "See all" page supports infinite scroll and filters.
- [ ] Preferences page roundtrips correctly; "reset to defaults" works.
- [ ] All strings i18n'd.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] No `any`.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Use TanStack Query's `refetchInterval` with a guard on `document.visibilityState`.
- The preferences matrix can be large (categories × channels). Consider a compact "category → row, channel → toggle" layout for mobile.

## 9. Suggested execution outline

1. Bell + dropdown (polling).
2. Notifications page.
3. Preferences page.
4. E2E test.
5. CHANGELOG.

## 10. Open questions / risks

- Risk: 60s polling drains battery on mobile. **Mitigation**: pause when tab hidden; document.
