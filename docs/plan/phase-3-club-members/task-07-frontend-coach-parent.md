# Task `P3-T07` — Frontend: Coach Profiles + Parent/Child Management

> **Phase**: 3 — Club & Member Management
> **Estimated size**: M
> **Depends on**: P3-T04, P3-T05, P3-T06
> **Can run in parallel with**: nothing significant

---

## 1. Context

Round out the member experience: coaches get a proper public profile screen and a place to edit theirs; parents get a place to see and manage their linked children.

## 2. Goal

> Players and Parents can browse coach profiles; Coaches can edit their own profile; Club Admins can link/unlink Parent–Child relations; Parents see a "My family" view of their children.

## 3. Scope

### In scope

- `features/coaches/`:
  - `pages/CoachListPage.tsx` — card grid of coaches with photo + name + specializations.
  - `pages/CoachDetailPage.tsx` — full profile + edit button (visible to self or Club Admin).
  - `components/CoachForm.tsx`.
  - `api/coaches.api.ts`.
- `features/family/`:
  - `pages/MyFamilyPage.tsx` — for Parents: list of linked children, quick links to their profiles & (later) bookings.
  - `components/ParentChildLinker.tsx` — Club Admin tool to link/unlink, available from a member detail page.
  - `api/relations.api.ts`.
- Member detail page (from P3-T06) gets a "Relations" tab showing incoming/outgoing relations.
- Role-aware nav:
  - "Coaches" in sidebar for any signed-in member.
  - "My family" for any member; if the member has no relations, show an empty state explaining what it is.
  - "Club" for Club Admin.

### Out of scope

- Booking flows → Phase 5.
- Coach availability calendar → Phase 4.

## 4. Inputs

- API contracts from P3-T04, P3-T05.

## 5. Deliverables

- All files listed.
- E2E test `frontend/tests/family.spec.ts`: Club Admin links a Parent to a Child; Parent signs in and sees the child in My Family; Parent opens the child's profile and edits the phone number.
- CHANGELOG.

## 6. Acceptance Criteria

- [ ] Coach list shows all active coaches with photo, name, top three specializations.
- [ ] A coach can edit their own profile; another coach cannot.
- [ ] Club Admin can link a Parent to a Child from the member detail page; the relation immediately appears under "Relations".
- [ ] Parent's "My Family" page lists all linked children; clicking one opens the child's profile.
- [ ] Unrelated members cannot see a child's PII.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] All strings i18n'd.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Empty states are important here — many tenants will have no parent/child relations at first. Make them inviting, not error-like.
- Coach photo URL: validate it client-side as a URL and show a placeholder avatar if it's empty or fails to load.

## 9. Suggested execution outline

1. Coaches feature (list + detail + edit).
2. Relations API + components.
3. Member detail "Relations" tab.
4. My Family page.
5. E2E test.

## 10. Open questions / risks

- none significant.
