# Task `P1-T03` — Docker & docker-compose Dev Environment

> **Phase**: 1 — Foundation
> **Estimated size**: M
> **Depends on**: P1-T01, P1-T02, P1-T05
> **Can run in parallel with**: P1-T04, P1-T07

---

## 1. Context

A developer should be able to clone the repo, run `docker compose up`, and see the React app talking to the API which talks to Postgres. This is the daily-driver dev experience and is also what we'll use to run smoke tests in CI.

## 2. Goal

> `docker compose up` from the repo root brings up Postgres, the API, and the Web app; the Web app at `http://localhost:5173` renders the API's `/api/v1/health` response.

## 3. Scope

### In scope

- Multi-stage `api.Dockerfile` (build with .NET 10 SDK image, run on the runtime image).
- Multi-stage `web.Dockerfile` (build with Node 20+ image, serve via Nginx or Vite preview).
- `docker/docker-compose.yml` with services: `postgres`, `api`, `web`. (Seq is added in P1-T07.)
- `.env.example` at repo root documenting required env vars; `.env` is gitignored.
- Health checks for each service so dependencies wait correctly.
- Volumes for Postgres data and (optional) `dotnet`/`npm` caches.
- A `docker/README.md` explaining the workflow.
- `Makefile` or `taskfile.yml` (pick one — recommend `Makefile` for simplicity) with `make up`, `make down`, `make logs`, `make reset-db`.

### Out of scope

- Production-grade Dockerfiles (slim base, non-root user, distroless, etc.) — basics only; harden in a later infra task.
- Kubernetes manifests / Helm charts — Future.
- HTTPS / mkcert in dev — Future.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/solution-structure.md`](../00-architecture/solution-structure.md)
- Env vars introduced:
  - `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`
  - `Postgres__ConnectionString` (composed from the above for the API container)
  - `VITE_API_BASE_URL` (e.g. `http://localhost:5080/api/v1` from the browser)
  - `ASPNETCORE_ENVIRONMENT=Development`

## 5. Deliverables

- `docker/api.Dockerfile`
- `docker/web.Dockerfile`
- `docker/docker-compose.yml`
- `.env.example`
- `Makefile` at repo root.
- `docker/README.md` describing the dev workflow.
- Update `CHANGELOG.md`.
- Update root `README.md` "How to run" section to make `make up` the primary path.

## 6. Acceptance Criteria

- [ ] On a fresh machine with Docker installed, `cp .env.example .env && make up` starts all services with no errors.
- [ ] `curl http://localhost:5080/api/v1/health` returns `200 { "status": "ok" }` within 30s of `make up`.
- [ ] Browser at `http://localhost:5173` shows a page that displays the health response (a placeholder is fine).
- [ ] `make down` cleanly stops everything; `make reset-db` wipes Postgres volume and restarts.
- [ ] Re-running `make up` after code changes rebuilds only what changed (caching works).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] No real secrets committed; `.env.example` uses placeholder values.
- [ ] CHANGELOG.md updated.
- [ ] README.md "How to run" rewritten around `make up`.

## 8. Handoff notes / gotchas

- The API container must wait for Postgres **healthy**, not just running. Use a Postgres healthcheck (`pg_isready`) and `depends_on: { postgres: { condition: service_healthy } }`.
- In dev, the API can apply EF migrations on startup; do not enable that in production.
- The web container can serve the built bundle via Nginx for parity; for the very first iteration, `vite preview` is acceptable. Document this.
- CORS: the API must allow `http://localhost:5173` in `Development`.
- Network: all services join one bridge network (`slotsmart-net`). The API talks to Postgres by service name (`postgres`); the browser talks to the API on the host port (`localhost:5080`).

## 9. Suggested execution outline

1. Write `api.Dockerfile` (build stage `mcr.microsoft.com/dotnet/sdk:10.0`, runtime `aspnet:10.0`).
2. Write `web.Dockerfile` (build stage `node:20-alpine`, runtime `nginx:alpine` serving `dist/`).
3. Write `docker-compose.yml` with `postgres`, `api`, `web`, healthchecks, volumes.
4. Add `.env.example` + Makefile.
5. Verify the round-trip from browser → API → DB works.
6. Document in `docker/README.md` and update root README.

## 10. Open questions / risks

- Risk: on Apple Silicon, some images lack ARM builds. **Mitigation**: stick to official `postgres:16-alpine`, `node:20-alpine`, and `mcr.microsoft.com/dotnet/*:10.0` — all multi-arch.
- Question: Nginx vs. `vite preview` for the web container? **Decision**: Nginx for parity with future hosting; the extra config is worth it.
