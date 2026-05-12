# AI Assistant Instructions for SlotSmart

> **Note**: This file provides instructions to AI coding assistants (Claude, GPT, etc.) about this project. It's automatically read by compatible AI IDEs.

## Project Overview

**Project Name**: SlotSmart 
**Type**: SaaS Web Application (Tennis Club Management) 
**Description**: A modern SaaS platform for tennis clubs, coaches, parents, and players to simplify club management, training scheduling, and communication. Multi-tenant architecture where each club operates independently.

## Technology Stack

**Backend**:
- ASP.NET Core (.NET 10) with REST API architecture
- Entity Framework Core with Clean Architecture
- PostgreSQL / SQL Server (configurable)
- JWT Authentication / OpenIddict / OAuth2
- UUIDv7 identifiers for all entities

**Frontend**:
- React with TypeScript
- Material UI (MUI)

**Infrastructure**:
- Docker support
- Git-based workflow
- CI/CD ready

## Project Structure

```
slotsmart/
├── docs/
│   ├── SUPERVISOR/          # AI Framework docs
│   │   ├── SUPERVISOR-FRAMEWORK.md
│   │   ├── WORKER-FRAMEWORK.md
│   │   ├── CURRENT-STATUS.md
│   │   ├── DELEGATION-TRACKER.md
│   │   ├── THINKING-LOG.md
│   │   └── DECISIONS-LOG.md
│   ├── plan/                # Development plans
│   └── project-description.md
├── .cursor/
│   ├── rules/               # Cursor rules
│   └── skills/              # Methodology skills
└── CHANGELOG.md
```

## Domain Entities

- **Member** - Club members with role-based access
- **Role** - Player | Parent | Coach | Head Coach | Club Admin
- **Coach** - Coach profiles and management
- **Training** - Group and individual training sessions
- **Booking** - Lesson bookings and cancellations
- **Club** - Multi-tenant club entity
- **MemberRelation** - Parent-child relationships

## AI Framework Integration

This project uses the **Supervisor-Worker AI Framework** for structured AI-assisted development.

### Framework Location
- Framework docs: `docs/SUPERVISOR/`
- Current status: `docs/SUPERVISOR/CURRENT-STATUS.md`
- Task tracker: `docs/SUPERVISOR/DELEGATION-TRACKER.md`

### Role Initialization

**To work as SUPERVISOR** (planning, delegating, validating):
```
Read docs/SUPERVISOR/SUPERVISOR-FRAMEWORK.md and initialize as SUPERVISOR
```

**To work as WORKER** (executing specific tasks):
```
Read docs/SUPERVISOR/WORKER-FRAMEWORK.md and initialize as WORKER
```

## Coding Standards

### General Rules
- Follow Clean Architecture principles (Domain, Application, Infrastructure, Presentation layers)
- Keep domain entities free of infrastructure concerns
- Use UUIDv7 for all entity identifiers
- Follow RESTful API conventions
- No hardcoded connection strings or credentials
- Write testable code with dependency injection

### C# Backend Conventions
- Use C# latest features (records, pattern matching, etc.)
- Follow Microsoft C# coding conventions
- Use async/await throughout (no sync-over-async)
- Use Result pattern or exceptions consistently for error handling
- DTOs for API contracts (never expose domain entities directly)
- Validators for all input (FluentValidation or similar)

### TypeScript Frontend Conventions
- Strict TypeScript (no `any` types)
- Functional React components with hooks
- MUI components for consistent UI
- Separate API layer from UI logic

### File Organization Rules
- Backend: `src/[Layer]/[Module]/[Feature]`
- Frontend: `src/[feature]/[component]`
- Tests mirror source structure

### Naming Conventions
- C#: PascalCase for classes, methods, properties
- TypeScript: camelCase for variables, PascalCase for components/types
- Database: snake_case for tables and columns

## Important Files

| File | Purpose |
|------|---------|
| `docs/project-description.md` | Full project requirements |
| `docs/SUPERVISOR/CURRENT-STATUS.md` | Current project health |
| `CHANGELOG.md` | Version history |

## Commands

```bash
# Development (once project is set up)
# dotnet run --project src/SlotSmart.Api

# Testing
# dotnet test

# Frontend
# npm run dev
```

## What NOT to Do

- Don't expose domain entities through API responses (use DTOs)
- Don't put business logic in controllers or repositories
- Don't skip updating CHANGELOG after changes
- Don't introduce new dependencies without discussion
- Don't change multi-tenant data isolation logic without architectural review
- Don't use sync blocking calls (`.Result`, `.Wait()`)

## Session Workflow

1. **Start**: Read `docs/SUPERVISOR/CURRENT-STATUS.md` for context
2. **During**: Follow framework protocols for your role
3. **End**: Update relevant docs, create handover if needed

---

**Last Updated**: 2026-05-12 
**Framework Version**: 1.3.1
