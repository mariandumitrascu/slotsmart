# Phase 6 — Communication & Notifications

> Release: **1.1** · Depends on: Phase 5 complete · Unlocks: a polished MVP.

## Goal

Add the **communication layer** that takes the MVP from functional to delightful: email notifications, in-app notifications, scheduled reminders, and schedule-change announcements. Notifications are pluggable so future channels (Push, SMS, Messenger) drop in with minimal change.

## Outcomes

- Each notification-worthy event (booking confirmed, cancelled, training cancelled, training moved, reminder, waitlist promoted, etc.) produces messages via the channels the recipient prefers.
- Email is delivered via a swappable SMTP provider (configurable; pluggable for SendGrid / Mailgun later).
- In-app notifications appear in a bell-icon dropdown and a notifications page; mark-as-read works.
- Per-user notification preferences let recipients opt in/out per category × channel.
- Reminders for upcoming trainings fire at configurable offsets (T-24h default).
- All notifications are templated with i18n support; templates have safe defaults and can be overridden per tenant in the future.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P6-T01 | [Notification model + dispatcher pipeline](./task-01-notification-model.md) | L | P5 done |
| P6-T02 | [Email channel (SMTP) + templating](./task-02-email-channel.md) | M | P6-T01 |
| P6-T03 | [In-app channel + UI](./task-03-in-app-channel.md) | M | P6-T01 |
| P6-T04 | [User notification preferences](./task-04-preferences.md) | M | P6-T01 |
| P6-T05 | [Booking & training event handlers](./task-05-event-handlers.md) | M | P6-T01..T04 |
| P6-T06 | [Scheduled reminders](./task-06-reminders.md) | M | P6-T05 |
| P6-T07 | [Frontend: notifications bell + page + prefs](./task-07-frontend.md) | L | P6-T03, P6-T04 |

## Acceptance criteria for the whole phase

- [ ] Confirming a booking sends an email (visible in MailHog/Mailpit in dev) AND creates an in-app notification.
- [ ] A user can mute "booking_confirmed" emails in preferences; subsequent bookings still create in-app notifications but no emails.
- [ ] A scheduled reminder for a training fires 24h before start time and creates a notification on each booked Player + their linked Parents.
- [ ] Cancelling a training broadcasts a notification to all current Confirmed bookers + the waitlist.
- [ ] In-app notifications bell shows unread count; clicking opens the dropdown; deep-link to the relevant page works.
- [ ] All emails are templated and i18n'd in at least English.
