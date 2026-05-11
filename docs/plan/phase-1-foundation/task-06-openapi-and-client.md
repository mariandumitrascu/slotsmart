# Task `P1-T06` — OpenAPI + Generated TypeScript Client

> **Phase**: 1 — Foundation
> **Estimated size**: M
> **Depends on**: P1-T01, P1-T05
> **Can run in parallel with**: P1-T03, P1-T04, P1-T07

---

## 1. Context

The OpenAPI document is the **contract** between backend and frontend. We want the frontend's API client to be generated from it so the two sides can never drift. Setting this up while the surface is tiny (one health endpoint) is much easier than retrofitting later.

## 2. Goal

> `dotnet run --project src/SlotSmart.Api` serves a complete OpenAPI 3.1 document at `/api/v1/openapi.json` and Swagger UI at `/api/v1/docs` in Development. The frontend has a generated, type-safe client and uses it (via TanStack Query) to call `/api/v1/health`.

## 3. Scope

### In scope

- Add OpenAPI generation to the API: prefer **Microsoft.AspNetCore.OpenApi** for .NET 10 (built-in), augmented with **Scalar** or **Swashbuckle Swagger UI** for the interactive doc.
- Annotate endpoints with summaries/descriptions; add response types and example responses.
- Configure problem details so `400` / `404` / `409` / `422` schemas are present in the spec.
- Frontend client generation: pick **one** of:
  - `openapi-typescript` (types only) + `openapi-fetch` (tiny runtime), or
  - `orval` (full hooks).
  - **Default recommendation: `openapi-typescript` + `openapi-fetch`** (smallest blast radius, perfect TS types, simple).
- An `npm run generate:api` script that regenerates the client from the running API or from a committed `openapi.json` snapshot.
- Commit the snapshot at `frontend/src/lib/openapi.json` so the build is reproducible without a running API.
- Rewrite `HealthPage` to call the API via the generated client + TanStack Query.

### Out of scope

- Real domain endpoints → respective phase tasks.
- API versioning machinery beyond the `/v1` prefix → if/when we hit `/v2`.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/api-conventions.md`](../00-architecture/api-conventions.md)
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
- Required services running: API only.

## 5. Deliverables

### Backend

- `Program.cs` registers OpenAPI + Swagger UI (or Scalar).
- `appsettings.Development.json` exposes the doc.
- Add `Microsoft.AspNetCore.OpenApi` + UI package via CPM.
- Document examples for the health endpoint as a reference for future endpoints.
- A small `OpenApi/` folder with document transformers (e.g. server URL, info block, security scheme placeholders).

### Frontend

- `frontend/package.json` adds: `openapi-typescript`, `openapi-fetch`, `tsx` (or `tsm`).
- `frontend/scripts/generate-api.ts` script:
  - Reads from `http://localhost:5080/api/v1/openapi.json` if available; otherwise from a committed snapshot.
  - Writes `frontend/src/lib/api-types.ts`.
- `frontend/src/lib/openapi.json` (committed snapshot).
- `frontend/src/lib/apiClient.ts` rewritten on top of `openapi-fetch` with the generated `paths` type.
- `HealthPage` uses TanStack Query + the generated client.

### Tests

- An API test that fetches `/api/v1/openapi.json` and asserts it parses as JSON and contains `info.title === "SlotSmart API"`.
- A frontend test that mocks the client and renders `HealthPage`.

### Docs

- `frontend/README.md` documents `npm run generate:api`.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] `GET /api/v1/openapi.json` returns a valid OpenAPI 3.1 document including the health endpoint.
- [ ] `GET /api/v1/docs` shows Swagger UI (or Scalar) listing the health endpoint.
- [ ] `npm run generate:api` regenerates `frontend/src/lib/api-types.ts` from the running API.
- [ ] `HealthPage` is fully typed — autocompletion for `data.status` works in editors.
- [ ] The committed snapshot `openapi.json` is updated in this PR.

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CI re-runs the generator and fails if the committed `api-types.ts` is stale.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Decide and commit to **one** OpenAPI generator. Mixing Swashbuckle and the new .NET 10 built-in OpenAPI complicates things.
- For the snapshot-vs-live decision, the script tries live first, then falls back to the committed snapshot — that way new contributors can generate even before they start the API.
- All endpoints from now on must include:
  - A summary (`/// <summary>` or `.WithSummary(...)`).
  - All possible response types via `.Produces<T>(StatusCodes.Status…)` / `.ProducesProblem(...)`.
  - At least one example.
- Add a "lint" step that runs `npx redocly lint frontend/src/lib/openapi.json` (or `spectral`) — optional but recommended.

## 9. Suggested execution outline

1. Add OpenAPI packages to API; wire `MapOpenApi()` and Swagger UI in Development.
2. Add response/example metadata to the health endpoint.
3. Write the API test verifying the spec.
4. Add `openapi-typescript` + `openapi-fetch` to the frontend.
5. Write `scripts/generate-api.ts` and the `npm run generate:api` script.
6. Commit the first `openapi.json` snapshot + generated `api-types.ts`.
7. Rewrite `apiClient.ts` and `HealthPage` to use the generated types.
8. Add the CI step that detects stale generated files.
9. Update CHANGELOG and README.

## 10. Open questions / risks

- Question: Scalar vs Swagger UI? **Decision**: Scalar — better DX, less legacy baggage. Note in CHANGELOG.
- Risk: the generated client may not handle problem+json error responses idiomatically. **Mitigation**: wrap `openapi-fetch` in `apiClient.ts` to normalize errors to a shared `ApiError` type used by the UI.
