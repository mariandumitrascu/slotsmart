# SlotSmart

Modern SaaS platform for tennis clubs — schedule trainings, manage members, take bookings.

> **Status**: Phase 1 (Foundation) in progress. The backend solution scaffolds and runs; database, Docker compose, CI, frontend, OpenAPI client, and observability arrive in P1-T02–T07. See [`docs/plan/`](docs/plan/) for the full roadmap and [`docs/SUPERVISOR/`](docs/SUPERVISOR/) for the supervised execution.

---

## Repository layout

```text
slotsmart/
├── backend/            ← .NET 10 Clean Architecture solution (P1-T01 onwards)
├── frontend/           ← React + Vite + MUI app (P1-T05)
├── docker/             ← docker-compose dev stack (P1-T03)
├── docs/
│   ├── plan/           ← phased delivery plan + architecture reference
│   ├── SUPERVISOR/     ← AI Supervisor-Worker framework, gates, handoff prompts
│   └── project-description.md
├── .github/workflows/  ← CI pipeline (P1-T04)
├── CHANGELOG.md
└── README.md           ← you are here
```

---

## Prerequisites

- **.NET SDK 10.0.x** (LTS, GA 2026-05-12). The repo's `backend/global.json` pins 10.0.300.
- macOS / Linux: any recent shell (use **bash**, not zsh, for compatibility with project scripts).
- Git.

### Install .NET 10 SDK

Pick one of:

```bash
# Option A — official Microsoft install script (per-user, no sudo).
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
```

```bash
# Option B — Homebrew (macOS).
brew install dotnet
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
```

```bash
# Option C — official .pkg installer for macOS.
# Download from https://dotnet.microsoft.com/en-us/download/dotnet/10.0
```

Verify:

```bash
dotnet --version    # → 10.0.x
dotnet --list-sdks  # must include a 10.0.x entry
```

---

## Build & run (backend only — P1-T01 deliverable)

```bash
cd backend
dotnet restore
dotnet build -warnaserror
dotnet test
dotnet run --project src/SlotSmart.Api --urls http://localhost:5080
# In another shell:
curl -fsS http://localhost:5080/api/v1/health
# → 200  {"status":"ok"}
```

The richer health endpoint (with database ping), Postgres, full OpenAPI, frontend, Docker compose and CI all land in subsequent P1 tasks. See the phase plan: [`docs/plan/phase-1-foundation/README.md`](docs/plan/phase-1-foundation/README.md).

---

## Conventions

- **Bash, not zsh / PowerShell.** Project scripts assume bash semantics.
- **Identifiers**: dual-key pattern (ADR-007). Public `EntityId : Guid` (UUIDv7) + hidden `bigint` surrogate primary key. See [`docs/plan/00-architecture/entity-identity.md`](docs/plan/00-architecture/entity-identity.md).
- **Solution file**: `backend/SlotSmart.slnx` (the modern .NET 10 XML solution format).
- **Central Package Management**: every `<PackageReference>` omits `Version=`; pin in `backend/Directory.Packages.props`.
- **Architecture rules**: enforced at build time by `backend/tests/SlotSmart.Architecture.Tests`. Layer violations break the build.
- **CHANGELOG**: follow [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/); newest first.

---

## Where to read next

| You want to… | Read |
| --- | --- |
| Understand the product | [`docs/project-description.md`](docs/project-description.md) |
| See the full roadmap | [`docs/plan/README.md`](docs/plan/README.md) |
| Find architecture decisions | [`docs/plan/00-architecture/`](docs/plan/00-architecture/) and [`docs/SUPERVISOR/DECISIONS-LOG.md`](docs/SUPERVISOR/DECISIONS-LOG.md) |
| See what's currently being worked on | [`docs/SUPERVISOR/CURRENT-STATUS.md`](docs/SUPERVISOR/CURRENT-STATUS.md) |
| Pick up a task as a worker agent | [`docs/SUPERVISOR/handoff-prompts/`](docs/SUPERVISOR/handoff-prompts/) |
| Verify a phase is complete | [`docs/SUPERVISOR/phase-gates/`](docs/SUPERVISOR/phase-gates/) |
