# Task `P6-T02` — Email Channel (SMTP) + Templating

> **Phase**: 6 — Communication & Notifications
> **Estimated size**: M
> **Depends on**: P6-T01
> **Can run in parallel with**: P6-T03, P6-T04

---

## 1. Context

We ship the Email channel for the notification dispatcher. SMTP is the lowest-common-denominator; in dev we run **Mailpit** (drop-in MailHog replacement) for a clickable inbox.

## 2. Goal

> A queued delivery with `Channel=Email` is sent via SMTP; the recipient sees the email in Mailpit in dev; the email's subject and body are templated, i18n'd, and include a deep link back to the app.

## 3. Scope

### In scope

- `INotificationChannelDispatcher` impl `EmailChannelDispatcher`:
  - Reads SMTP config from `appsettings`.
  - Uses **FluentEmail** with `Razor` renderer (or **Scriban**; pick one — recommend FluentEmail + Razor for familiarity).
  - Renders a template named by category (e.g. `BookingConfirmed.cshtml`) with the `Data` dictionary from the `Notification`.
  - Sets the `From` to a configured tenant-or-platform address; subject pulled from a resource file.
  - Includes a tracking-friendly `Message-ID` header.
- Templates live in `Infrastructure/Notifications/Templates/Email/<category>/`:
  - `en/Subject.txt` + `en/Body.cshtml`.
  - Structure ready for future locales.
- Mailpit added to `docker-compose.yml` (port `8025` UI, `1025` SMTP) under the `observability` profile (or its own `mail` profile).
- Resilience: timeouts, retries on transient SMTP errors; permanent errors → `Failed`.
- A simple "send test email" admin endpoint `POST /api/v1/admin/notifications:test-email` (ClubAdmin) that fires a fixture notification to themselves.

### Out of scope

- Email click-tracking / pixel tracking → V2.
- Bounce/complaint handling → V2.
- Provider switch (SendGrid/Mailgun) → V2.
- Unsubscribe-link compliance for cold emails → not needed (we only send transactional in MVP).

## 4. Inputs

- Env vars:
  - `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__Tls=true|false`.
  - `Notifications__Email__FromAddress`, `Notifications__Email__FromName`.
  - `Notifications__Email__AppBaseUrl` (e.g. `https://app.slotsmart.local:5173` or club subdomain in prod).
- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)

## 5. Deliverables

- `Infrastructure/Notifications/Email/EmailChannelDispatcher.cs`.
- `Infrastructure/Notifications/Email/Templates/<category>/en/...` (at least: `BookingConfirmed`, `BookingCancelled`, `TrainingReminder`, `TrainingCancelled`, `TrainingChanged`, `WaitlistPromoted`, `InvitationSent`).
- `docker-compose.yml` adds Mailpit (profile `mail`).
- Admin "send test" endpoint.
- Tests:
  - With a fake SMTP collector (MailKit + a local TCP listener, or use Mailpit in CI) verify a queued delivery results in a sent email.
  - Template renders with correct subject + body + deep link.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] `make up` with the `mail` profile starts Mailpit at `http://localhost:8025`.
- [ ] Triggering a booking confirmation in dev results in an email visible in Mailpit with the booking details.
- [ ] The email contains a deep link `${AppBaseUrl}/app/bookings/{id}` that opens the booking in the app.
- [ ] Subject + body are translated through resource files (English seeded; placeholder for additional locales).
- [ ] Test-email admin endpoint works.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Don't embed raw URLs in templates — call a helper `Url.Notification("BookingConfirmed", new { BookingId = ... })` so deep-link generation is centralized.
- Email body is **HTML + plain-text alternative**. Render both; many clients strip HTML.
- For the dev FromAddress, use `noreply@slotsmart.local`. Document that production needs proper SPF/DKIM in V2.

## 9. Suggested execution outline

1. Install FluentEmail + Razor; create dispatcher.
2. Author the 7 templates + subjects.
3. Mailpit in compose.
4. Admin test endpoint.
5. Tests.
6. CHANGELOG.

## 10. Open questions / risks

- Risk: rendering Razor at runtime requires the `RazorLight` (or similar) package; verify .NET 10 compatibility. **Mitigation**: if Razor is awkward, switch to **Scriban** templates (text-based, no runtime compile) — same template surface, easier to ship.
