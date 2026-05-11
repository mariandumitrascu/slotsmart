# Task `P3-T06` — Frontend: Club Settings, Member Directory, Invitations

> **Phase**: 3 — Club & Member Management
> **Estimated size**: L
> **Depends on**: P3-T01, P3-T02
> **Can run in parallel with**: P3-T07 (parts)

---

## 1. Context

The first real product surface. Club Admins need a comfortable place to manage settings and people; Coaches need a directory.

## 2. Goal

> A Club Admin can manage the club's settings, see all members in a paginated table, filter/search them, invite new ones, edit profiles, deactivate, and re-send invitations.

## 3. Scope

### In scope

- Feature folder `frontend/src/features/members/`:
  - `pages/MemberDirectoryPage.tsx`
  - `pages/MemberDetailPage.tsx`
  - `pages/InviteMemberPage.tsx` (or a modal)
  - `components/MemberTable.tsx`, `MemberFilters.tsx`, `MemberForm.tsx`, `RoleSelect.tsx`, `InvitationLinkDialog.tsx`.
  - `api/members.api.ts` (TanStack query/mutation hooks built on generated client).
- Feature folder `frontend/src/features/club-settings/`:
  - `pages/ClubSettingsPage.tsx` with tabs: General, Working Hours, Booking Defaults.
  - `api/club.api.ts`.
- Public invitation acceptance page (no auth required): `pages/AcceptInvitationPage.tsx` — sets password + first/last name, signs the user in on success.
- Routing:
  - `/app/members` (Coach+; Club Admin sees full toolbar incl. Invite).
  - `/app/members/:id` (Coach+; self for own page; Club Admin can edit any).
  - `/app/club` (Club Admin).
  - `/invitations/accept?token=...` (public).
- UX details:
  - Member table: sortable, paginated, virtualized (or simple pagination — pick simple), filters by role/status, debounced search.
  - Invite flow: choose role(s), email, optional first/last name, on success show a copyable invitation URL (since email may not be wired yet).
  - Deactivate: confirmation dialog with explicit "Yes, deactivate".
  - i18n: every visible string.
  - Optimistic update for role changes and deactivate, with rollback on error.

### Out of scope

- Coach profile UI → P3-T07.
- Parent/child UI → P3-T07.
- Email rendering / send → Phase 6.

## 4. Inputs

- API contracts from P3-T01 / P3-T02 (generated client).
- Architecture docs:
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)

## 5. Deliverables

- All files listed above.
- E2E test `frontend/tests/members.spec.ts` covering: invite → accept → directory shows new member → admin changes their roles → deactivate.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Club Admin can invite a member, see the invite in pending state, copy the invite URL, paste it in another browser and accept it.
- [ ] After acceptance, the new member appears in the directory as `Active`.
- [ ] Club Admin can change the new member's roles and the change is reflected on refresh.
- [ ] Search and filters update the URL (so links can be shared).
- [ ] Club settings page edits roundtrip successfully (PATCH).
- [ ] Non-admins see the directory in read-only mode (no Invite/Edit/Deactivate actions).
- [ ] All errors surface as MUI snackbars + inline form errors; no raw error strings.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] All strings i18n'd.
- [ ] No `any`.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- The role select must enforce ≥1 role (matches backend invariant) — disable "remove last" client-side and rely on backend error as a safety net.
- The invitation accept page must not show "you're already signed in"; if the user is signed in, sign them out before showing the form.
- Use `<DataGrid>` from MUI X for the table if available; otherwise a stock table is fine. Pick one and stick to it; document.

## 9. Suggested execution outline

1. Generated client refresh.
2. `members.api.ts` hooks.
3. Directory page + filters + table.
4. Invite + accept pages.
5. Member detail + edit forms.
6. Club settings page.
7. E2E test.

## 10. Open questions / risks

- Question: bulk invite (CSV)? **Decision (MVP)**: no — single invite only.
