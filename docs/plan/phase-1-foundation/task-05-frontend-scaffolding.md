# Task `P1-T05` — Frontend Scaffolding (Vite + React + TS + MUI)

> **Phase**: 1 — Foundation
> **Estimated size**: M
> **Depends on**: P1-T01
> **Can run in parallel with**: P1-T02, P1-T04, P1-T07

---

## 1. Context

Stand up the React app under `frontend/` with the conventions from [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md) and [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md). No business features yet — just the app shell, routing, theming, and a placeholder page that renders the API's `/api/v1/health` response.

## 2. Goal

> `cd frontend && npm install && npm run dev` starts the app on `http://localhost:5173`, which renders a layout with header/sidebar/content and shows the API health status fetched from `http://localhost:5080/api/v1/health`.

## 3. Scope

### In scope

- `frontend/` initialized with Vite (React + TypeScript template).
- Strict TypeScript config.
- ESLint + Prettier + lint-staged + Husky pre-commit (lint + typecheck on staged files).
- Vitest + React Testing Library set up; one trivial passing test.
- React Router v6+ data router with a simple route tree (`/`, `/health` placeholder page).
- Material UI v6+, with a `theme/` folder, a `ThemeProvider`, a CssBaseline, and a starter palette + typography.
- TanStack Query client wired up via `QueryClientProvider`.
- Zustand store stub (empty) in `lib/store.ts`.
- React Hook Form + Zod installed and a tiny demo (not visible by default) to validate it works.
- `react-i18next` set up with an English `common.json` namespace.
- An app shell with `Header`, `Sidebar` (collapsible), and a `Outlet`-based content area, all using MUI.
- An `apiClient.ts` in `src/lib/` that wraps `fetch` with the base URL from `import.meta.env.VITE_API_BASE_URL`. **No** generated client yet — that comes with **P1-T06**.
- Path aliases in `tsconfig.json` + `vite.config.ts` (`@/app`, `@/features`, `@/shared`, `@/lib`).
- `npm run` scripts: `dev`, `build`, `preview`, `test`, `test:watch`, `lint`, `lint:fix`, `typecheck`, `format`.

### Out of scope

- Generated OpenAPI client → **P1-T06**.
- Authentication UI → **P2-T05**.
- Any real feature pages → Phase 3+.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Env vars introduced:
  - `VITE_API_BASE_URL` — base URL for API calls from the browser.

## 5. Deliverables

- `frontend/package.json` with all dependencies pinned via `package-lock.json`.
- `frontend/tsconfig.json`, `frontend/tsconfig.app.json`, `frontend/tsconfig.node.json`.
- `frontend/vite.config.ts`.
- `frontend/.eslintrc.cjs` or `eslint.config.js`, `frontend/.prettierrc`.
- `frontend/src/main.tsx`, `frontend/src/app/AppProviders.tsx`, `frontend/src/app/routes.tsx`.
- `frontend/src/app/theme/index.ts`.
- `frontend/src/app/layout/AppShell.tsx`, `Header.tsx`, `Sidebar.tsx`.
- `frontend/src/lib/queryClient.ts`, `frontend/src/lib/apiClient.ts`, `frontend/src/lib/i18n.ts`.
- `frontend/src/features/health/HealthPage.tsx` calling `/api/v1/health` and rendering the response.
- `frontend/src/locales/en/common.json` with a few seed strings.
- `frontend/.env.example` with `VITE_API_BASE_URL=http://localhost:5080/api/v1`.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] `npm install && npm run dev` runs without errors.
- [ ] `http://localhost:5173/` shows the app shell.
- [ ] Navigating to `http://localhost:5173/health` (or showing it on `/`) renders the JSON response from the API.
- [ ] `npm run build` produces a clean production bundle.
- [ ] `npm run lint`, `npm run typecheck`, `npm run test` all pass.
- [ ] Husky pre-commit hook runs lint + typecheck on staged files; bypass with `--no-verify` is documented in `frontend/README.md`.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] `frontend/README.md` explains scripts and conventions.
- [ ] No `any` types in scaffolding code.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Pin Node version via `frontend/.nvmrc` (Node 20 LTS).
- MUI v6 uses ESM by default; configure Vite accordingly (no special config usually needed).
- `useQuery` for the health call so the page also demonstrates TanStack Query + the loading / error states.
- The `Sidebar` should be collapsible and remember its state in `localStorage` via Zustand persistence.
- Use the **theme palette** for all colors in the shell; do not hard-code hex.
- `i18n` is set up but every visible string in the shell goes through `t(...)`.

## 9. Suggested execution outline

1. `npm create vite@latest frontend -- --template react-ts`.
2. Install deps: MUI, TanStack Query, React Router, Zustand, RHF, Zod, react-i18next, ESLint plugins, Prettier, Vitest, RTL, Husky, lint-staged.
3. Configure TS strict + path aliases.
4. Add Theme + AppProviders + AppShell.
5. Add `HealthPage` and route.
6. Add `apiClient.ts` (thin wrapper around fetch + JSON + error normalization).
7. Set up Husky + lint-staged.
8. Write `frontend/README.md` and update root `CHANGELOG.md`.

## 10. Open questions / risks

- Question: dark mode now or later? **Decision**: theme infra ready, toggle UI deferred to Phase 3 polish.
- Risk: MUI bundle size. **Mitigation**: tree-shake by importing from `@mui/material/Button` etc. when icons start landing; not a problem at this stage.
