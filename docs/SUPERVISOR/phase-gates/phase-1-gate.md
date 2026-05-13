# Phase Gate G1 — Foundation → Auth & Tenancy

> **Gate purpose**: Verify the runnable scaffolding is solid before any feature work begins.
> **Source phase plan**: [`../../plan/phase-1-foundation/README.md`](../../plan/phase-1-foundation/README.md)
> **Status**: ⏳ Not yet run

---

## 1. Pre-conditions (task completion)

All seven Phase 1 tasks must be `✅ COMPLETED` in [`../DELEGATION-TRACKER.md`](../DELEGATION-TRACKER.md):

- [ ] **P1-T01** Repository & solution scaffolding
- [ ] **P1-T02** Database, EF Core, migrations bootstrap
- [ ] **P1-T03** Docker & docker-compose dev environment
- [ ] **P1-T04** CI pipeline (GitHub Actions)
- [ ] **P1-T05** Frontend scaffolding (Vite + MUI + Router)
- [ ] **P1-T06** OpenAPI + generated TS client + shared health endpoint
- [ ] **P1-T07** Observability baseline (Serilog + OpenTelemetry + Seq)

---

## 2. Automated verification

Run from a **fresh clone** of the repo. Expected: all commands exit `0`.

### 2.1 Backend builds and tests pass

```bash
cd backend
dotnet restore
dotnet build --no-restore -warnaserror
dotnet test --no-build --logger "console;verbosity=normal"
```

**Expected**: `Build succeeded. 0 Warning(s) 0 Error(s)`. All test projects report `Passed`. Architecture tests must include layer-rule assertions (P1-T01 §6).

### 2.2 Frontend builds, tests, lints

```bash
cd frontend
npm ci
npm run lint
npm run test -- --run
npm run build
```

**Expected**: Each command exits `0`. No `any` types in new code (TypeScript strict).

### 2.3 Docker compose stack boots

```bash
cd docker
docker compose up -d
sleep 20
docker compose ps
```

**Expected**: Services `postgres`, `api`, `web`, and `seq` (or chosen log sink) are all `Up` / `healthy`.

### 2.4 End-to-end health probe

```bash
# API health
curl -fsS http://localhost:5080/api/v1/health
# Expected JSON body: {"status":"ok"} (or richer body including DB ping per P1-T06)

# Web app reachable
curl -fsSI http://localhost:5173 | head -1
# Expected: HTTP/1.1 200 OK
```

**Expected**: Both curls succeed. Open `http://localhost:5173` in a browser → React app renders the health response from the API.

### 2.5 OpenAPI document is published and frontend client matches

```bash
curl -fsS http://localhost:5080/openapi/v1.json -o /tmp/openapi.json
# Confirm the generated TS client in frontend/src/lib/api/* was generated against this spec
git -C frontend diff --quiet src/lib/api && echo "client up to date" || echo "client drifted"
```

**Expected**: `client up to date`.

### 2.6 Observability emits structured logs

```bash
curl -s http://localhost:5080/api/v1/health > /dev/null
docker compose logs api --tail=20 | grep -E '"level":(.*)|"@l":' || echo "structured logs missing"
```

**Expected**: At least one structured (Serilog/Otel) log line with `@l` or `level`. Seq UI at `http://localhost:5341` (or chosen) shows the request span.

### 2.7 CI is green on the latest branch

Check GitHub Actions for the most recent push to the working branch:

```bash
gh run list --branch "$(git branch --show-current)" --limit 1
```

**Expected**: Most recent run is `completed success`.

---

## 3. Manual verification

- [ ] Visiting `http://localhost:5173` in a browser shows a page that renders the API health response (proves React → API → Postgres path).
- [ ] Stopping `postgres` and reloading the page surfaces a friendly error (proves error handling baseline).
- [ ] `seq` (or chosen log UI) shows the request that produced the page render with a single trace ID across web and api.

---

## 4. Promotion checklist (SUPERVISOR ticks)

- [ ] All §1 tasks marked `✅ COMPLETED` in DELEGATION-TRACKER.md
- [ ] All §2 commands ran clean — outputs captured in §6 run log
- [ ] All §3 manual checks confirmed
- [ ] `CHANGELOG.md` has entries for all 7 tasks under `Added`
- [ ] No `TODO` / `FIXME` left without a tracked follow-up
- [ ] DECISIONS-LOG has ADRs for any deviations (e.g. .NET version)
- [ ] THINKING-LOG has a "Phase 1 closed" entry

If all boxes are ticked → mark this gate `✅ PASSED` in [`./README.md`](./README.md) and create the first Phase 2 handoff prompt.

---

## 5. Failure response

If any item fails:

1. Mark the corresponding task `🚧 IN PROGRESS` in DELEGATION-TRACKER.md.
2. Capture the exact failing command + output in `SESSION-SUMMARIES/PHASE-1-GATE-RUN-[date].md`.
3. Issue a remediation handoff prompt to a worker citing the failure (use [`../handoff-prompts/_template.md`](../handoff-prompts/_template.md)).
4. **Do not advance the phase.**

---

## 6. Run log

| Date | Result | Evidence | Supervisor notes |
|---|---|---|---|
| _empty_ | — | — | — |

---

**Gate version**: 1.0
**Source plan version**: matches Phase 1 README at time of writing
