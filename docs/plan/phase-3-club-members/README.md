# Phase 3 — Club & Member Management

> Release: **1.0 (MVP)** · Depends on: Phase 2 complete · Unlocks: Training (Phase 4) and Booking (Phase 5).

## Goal

Give a Club Admin the tools to run the **people** side of their club: profiles, roles, coaches, and parent–child relationships.

## Outcomes

- A `Member` aggregate exists per tenant; every `ApplicationUser` is paired with exactly one `Member` in its tenant.
- Club Admins can invite/create members, edit their profiles, assign roles, and deactivate them.
- Coaches have a richer profile (specialization, bio, photo URL).
- Parents can be linked to one or more child Players; children that are minors can only be booked by their Parent.
- The Club Admin can manage the club's general settings (name, time zone, working hours, default booking policies).
- Frontend has all the screens to do the above.

## Tasks

| ID | Title | Size | Depends on |
|----|-------|------|------------|
| P3-T01 | [Club settings domain & endpoints](./task-01-club-settings.md) | M | P2 done |
| P3-T02 | [Member aggregate + CRUD + invitations](./task-02-member-crud.md) | L | P3-T01 |
| P3-T03 | [Role management on Member](./task-03-member-roles.md) | M | P3-T02 |
| P3-T04 | [Coach profile extension](./task-04-coach-profile.md) | M | P3-T02 |
| P3-T05 | [Parent–Child relations (MemberRelation)](./task-05-parent-child.md) | M | P3-T02 |
| P3-T06 | [Frontend: club settings, member directory, invitations](./task-06-frontend-members.md) | L | P3-T01, P3-T02 |
| P3-T07 | [Frontend: coach profiles + parent/child management](./task-07-frontend-coach-parent.md) | M | P3-T04, P3-T05, P3-T06 |

## Acceptance criteria for the whole phase

- [ ] A Club Admin can invite a new member by email; the invitee receives an invitation token, accepts, sets a password, and lands as a Member of the club with the chosen role(s).
- [ ] A Club Admin can change a Member's roles; Roles in the JWT are reissued on the next sign-in (no live revocation in MVP).
- [ ] A Club Admin can deactivate a Member (`IsActive=false`); deactivated members cannot sign in.
- [ ] Coaches have a profile screen with bio, specialization, photo URL.
- [ ] A Parent can be linked to N child Players, and the child's bookings reflect the Parent on the booking record (will mature in Phase 5).
- [ ] All endpoints have policies; cross-tenant access returns 404.
