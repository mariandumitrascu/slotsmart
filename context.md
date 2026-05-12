# Project Context for AI Assistants
# SlotSmart

> This file provides essential context for AI coding assistants working on this project.

## Quick Summary

**What is this project?** 
SlotSmart is a modern SaaS platform for tennis clubs, coaches, parents, and players. It simplifies club management, training scheduling, and communication. The system is a multi-tenant solution where each tennis club operates as an independent organization with its own members, coaches, schedules, and permissions.

**Current Phase**: Foundation / Initial Development 
**Health Status**: 🟡 Early Stage (Framework initialized, development not started)

## Tech Stack

| Category | Technology |
|----------|------------|
| Language (Backend) | C# (.NET 10) |
| Framework | ASP.NET Core |
| ORM | Entity Framework Core |
| Database | PostgreSQL / SQL Server |
| Testing (Backend) | xUnit / NUnit |
| Language (Frontend) | TypeScript |
| Frontend Framework | React |
| UI Library | Material UI (MUI) |
| Auth | JWT / OpenIddict / OAuth2 |
| IDs | UUIDv7 |

## Architecture Overview

Clean Architecture with 4 layers:
- **Domain**: Entities, Value Objects, Domain Events, interfaces
- **Application**: Use Cases, DTOs, Validators, Application Services
- **Infrastructure**: EF Core, Repositories, External Services
- **Presentation**: ASP.NET Core Controllers, Middleware

Multi-tenant with data isolation per club.

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│        (ASP.NET Core REST API)          │
├─────────────────────────────────────────┤
│          Application Layer              │
│     (Use Cases, DTOs, Validators)       │
├─────────────────────────────────────────┤
│            Domain Layer                 │
│    (Entities, Business Logic, Events)   │
├─────────────────────────────────────────┤
│         Infrastructure Layer            │
│   (EF Core, Repos, External Services)   │
└─────────────────────────────────────────┘
```

## Key Domain Concepts

### Multi-Tenancy
Each Club is an independent tenant. Data is isolated per club. Members, coaches, trainings, and bookings all belong to a specific club.

### Member Roles
- **Player**: Books and attends trainings
- **Parent**: Manages children's bookings and attendance
- **Coach**: Manages individual/group training sessions
- **Head Coach**: Manages coaches and overall program
- **Club Administrator**: Manages club settings, members, permissions

### Core Entities
- `Member` - All club participants (has a Role)
- `Club` - Multi-tenant root entity
- `Coach` - Coach profile linked to Member
- `Training` - Session (group or individual, recurring or one-time)
- `Booking` - Member enrolled in a Training
- `MemberRelation` - Parent-child relationships between Members

## Directory Guide

| Directory | Contains |
|-----------|----------|
| `docs/` | Project docs, plans, AI framework |
| `docs/SUPERVISOR/` | AI Supervisor-Worker Framework files |
| `docs/plan/` | Development plans |
| `.cursor/skills/` | Methodology skills for AI |
| `.cursor/rules/` | Cursor IDE rules |

## Current Work Focus

Based on `docs/SUPERVISOR/CURRENT-STATUS.md`:

### Active Tasks
None - Framework just initialized, ready for development planning.

### Recent Changes
2026-05-12: AI Supervisor-Worker Framework installed.

### Known Issues
None currently.

## AI Framework

This project uses the **Supervisor-Worker AI Framework**.

- **Framework docs**: `docs/SUPERVISOR/`
- **Initialize as Supervisor**: For planning and delegating
- **Initialize as Worker**: For executing specific tasks

Always check `docs/SUPERVISOR/CURRENT-STATUS.md` before starting work.

## Quick Commands

```bash
# Start development (once scaffolded)
# dotnet run

# Run tests
# dotnet test

# Start frontend
# npm run dev
```

## Important Notes

- All entities use UUIDv7 identifiers
- Domain layer has NO dependency on infrastructure
- Controllers are thin - business logic lives in Application layer
- Multi-tenant isolation is critical - always filter by ClubId
- JWT authentication with role-based permissions

---

**Auto-generated**: 2026-05-12 
**Update this file** when major project changes occur.
