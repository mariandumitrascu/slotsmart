# Task `P1-T04` — CI Pipeline (GitHub Actions)

> **Phase**: 1 — Foundation
> **Estimated size**: S
> **Depends on**: P1-T01 (backend builds), ideally P1-T05 (frontend builds) for the web job
> **Can run in parallel with**: P1-T02, P1-T03, P1-T07

---

## 1. Context

We want fast, predictable CI from day one: every PR is built, tested, and linted. The same workflow is used to build Docker images on the main branch (no push to a registry yet — that's a CD concern).

## 2. Goal

> Pushing a PR runs three jobs — `backend`, `frontend`, `docker-build` — and all three pass on the seeded scaffolding.

## 3. Scope

### In scope

- `.github/workflows/ci.yml` with jobs:
  - **backend**: `actions/setup-dotnet`, restore, build (`-warnaserror`), test (with `--collect:"XPlat Code Coverage"`), upload coverage as artifact.
  - **frontend**: `actions/setup-node`, `npm ci`, `npm run lint`, `npm run typecheck`, `npm run test -- --run`, `npm run build`.
  - **docker-build**: builds `api.Dockerfile` and `web.Dockerfile` (without pushing) to ensure they remain buildable.
- Concurrency group `ci-${{ github.ref }}` cancel-in-progress on PRs.
- Cache for NuGet and npm.
- Required status check enforcement instructions in `docs/plan/phase-1-foundation/task-04-ci-pipeline.md` (this file) — actually applied by the repo owner manually in GitHub settings.
- `.github/dependabot.yml` (or `renovate.json`) configured for NuGet, npm, GitHub Actions, Docker.

### Out of scope

- Pushing images to a container registry → CD task, post-MVP.
- Deployments → CD task.
- Code coverage gates → after we have meaningful coverage in Phase 2/3.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md)
- Secrets required in CI: none for MVP. Document any future secrets in `docs/plan/phase-1-foundation/task-04-ci-pipeline.md` "Open questions".

## 5. Deliverables

- `.github/workflows/ci.yml`
- `.github/dependabot.yml`
- A `Directory.Build.props` tweak (if not already) to keep CI builds deterministic (`<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>`).
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] Pushing a PR with the existing scaffolding triggers `backend`, `frontend`, `docker-build` and all three pass.
- [ ] The backend job runs Testcontainers-based tests against Postgres (the GitHub Actions runner supports Docker natively).
- [ ] A PR that introduces a compiler warning fails the backend job.
- [ ] A PR that introduces a TypeScript or ESLint error fails the frontend job.
- [ ] Cache hits are visible in subsequent runs (NuGet + npm).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] CI workflow has the concurrency cancel-in-progress for PRs.
- [ ] Dependabot enabled and runs weekly.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Use `setup-dotnet` with `global-json-file: backend/global.json` so the workflow honors the SDK pin.
- Restore separately from build to maximize cache hits: `dotnet restore` then `dotnet build --no-restore`.
- Run `dotnet test --no-build --logger trx` so artifacts can be uploaded; consider `dorny/test-reporter` for nice PR comments later.
- The `docker-build` job should `docker buildx build --load` (no push) to verify both Dockerfiles still build.
- Don't reduce parallelism by combining the three jobs — keep them separate so signal is clear.

## 9. Suggested execution outline

1. Author `ci.yml` with the three jobs.
2. Add caching for NuGet (`~/.nuget/packages`) and npm (handled by `setup-node` with `cache: npm`).
3. Add Dependabot config.
4. Open a sample PR to verify the workflow runs green.
5. Update CHANGELOG.md.

## 10. Open questions / risks

- Question: do we want code coverage gates now? **Decision**: not yet. Re-evaluate at end of Phase 3.
- Risk: Testcontainers slow on cold runners. **Mitigation**: image is pulled once per job; acceptable for now. Consider self-hosted runners post-MVP if it becomes a pain point.
