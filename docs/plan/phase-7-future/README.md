# Phase 7 — Future (V2+)

> Release: **2.0+** · Depends on: Phases 1–6 complete. Outline only — **do not** plan at task level until each is selected for a release.

The product description includes a "Future Plans" section. This document captures, for each item, the **shape** of the future work: the goals, the major sub-problems, and the dependencies on the MVP. When prioritized, each item should be split out into its own phase folder (e.g. `phase-8-payments/`) and turned into agent-ready tasks using the template in [`../_templates/task-prompt-template.md`](../_templates/task-prompt-template.md).

---

## 7.1 Mobile application

**Goal**: a React Native (or Expo) app that mirrors the booking/calendar/attendance experience and uses push notifications.

**Sub-problems**:

- Share API client + types with web via a workspace package.
- Native auth flow (Authorization Code + PKCE, secure storage of refresh token).
- Push notifications (FCM/APNs) integrated with the notification dispatcher (new channel `Push`).
- Offline-capable "today's trainings" view.

**Dependencies**: stable web product (post-Phase 6); SPA migrated to PKCE flow.

---

## 7.2 AI-powered scheduling recommendations

**Goal**: suggest training slots, coach assignments, and pricing based on demand history.

**Sub-problems**:

- Feature store of historical bookings, attendance, cancellations.
- Demand forecasting per training profile.
- "Suggested next session" UI for Head Coaches.
- Privacy / opt-out controls.

**Dependencies**: ≥ 3 months of operational data; analytics dashboard (7.5) for grounded UX.

---

## 7.3 Automated slot optimization

**Goal**: given a club's coach availability, courts, and player preferences, propose an optimal weekly schedule.

**Sub-problems**:

- Coach availability calendar (V2).
- Player preference capture.
- Court entity + scheduling constraint model.
- Optimizer (constraint programming, OR-Tools-style; possibly a hosted micro-service).

**Dependencies**: court/facility entity (7.4); coach availability domain.

---

## 7.4 Court reservations

**Goal**: model courts as bookable resources, both for trainings (auto-reserved) and for ad-hoc member reservations.

**Sub-problems**:

- `Court` aggregate per tenant: number, surface, indoor/outdoor.
- Conflict detection (court-overlap like coach-overlap).
- Pricing rules per slot/court (couples with 7.6 Payments).
- New "Book a court" UI distinct from training booking.

**Dependencies**: domain model extension; new authorization rules.

---

## 7.5 Analytics dashboard

**Goal**: club-level KPIs (attendance rate, late cancellations, coach utilization, churn).

**Sub-problems**:

- Reporting read model (materialized views or a small lake).
- Recurring rollups (daily/weekly Quartz jobs).
- Dashboard pages with charts.
- Export to CSV.

**Dependencies**: clean event stream from outbox; sufficient data accumulated.

---

## 7.6 Payment integration

**Goal**: charge for bookings, season passes, court reservations.

**Sub-problems**:

- Choose provider (Stripe primary; document trade-offs).
- Add `Money` and `Currency` to domain (already laid groundwork in P3-T01).
- Booking → invoice flow; refunds; payment webhooks.
- Compliance (PCI, GDPR data minimization, tax — country-by-country).
- Tenant-level connect / standard onboarding.

**Dependencies**: stable booking flow; SCA compliance design.

---

## 7.7 Tournament management

**Goal**: register tournaments, brackets, results, leaderboards.

**Sub-problems**:

- Domain (Tournament, Match, Bracket, Result).
- Bracket generator (single elim, double elim, round-robin).
- Public-facing tournament pages for spectators.
- Integration with attendance (no-shows, walkovers).

**Dependencies**: solid member and player data; reporting layer.

---

## 7.8 Coach performance tracking

**Goal**: surface metrics that help Head Coaches manage their teams.

**Sub-problems**:

- Define metrics (attendance rate of their players, cancellations per coach, hours coached, retention).
- Privacy framing (coaches see their own; HeadCoach sees team; ClubAdmin sees all).
- Dashboard pages.

**Dependencies**: analytics platform from 7.5.

---

## 7.9 Cross-cutting V2 candidates (not in the original description)

These keep coming up in the MVP planning — promote when they become priorities:

- **Multi-tenant identity** (one human at multiple clubs).
- **Authorization Code + PKCE** SPA flow (replaces ROPC in P2-T02).
- **Email bounce/complaint handling** + dedicated transactional provider.
- **Push notifications** (mobile + browser).
- **SMS notifications** (Twilio or similar).
- **GDPR data export / deletion** flows.
- **Multi-region / data residency**.
- **Self-service plan management & billing UI**.
- **Public club pages** (SEO, allow non-members to discover).
- **Mass actions / bulk imports** (member CSV, schedule import).
- **Calendar feed (iCal) per user**.
- **Holiday / non-working day catalog** for training series.

---

## Process to turn an item into a phase

1. Decide the release window (e.g. "V2.0 includes 7.6 Payment integration").
2. Create a `phase-N-<slug>/` folder.
3. Write a `README.md` with goals, dependencies, and a task table.
4. Split into agent-ready task prompts using [`../_templates/task-prompt-template.md`](../_templates/task-prompt-template.md).
5. Add a row to the master phase table in [`../README.md`](../README.md).
6. Update `CHANGELOG.md` under `Changed → Plan`.
