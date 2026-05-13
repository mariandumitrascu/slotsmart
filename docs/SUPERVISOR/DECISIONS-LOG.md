# Technical Decisions Log
## SlotSmart - Architecture & Implementation Decisions

**Purpose**: Record technical decisions with context, alternatives, and reasoning. 
**Usage**: Add entries when making significant technical choices that affect architecture, implementation, or integration.

---

## 📝 DECISION RECORD FORMAT

Each decision should be documented as an **ADR (Architecture Decision Record)**:

```markdown
### ADR-[NUMBER]: [Title]

**Date**: [Date]  
**Status**: [Proposed | Accepted | Deprecated | Superseded by ADR-X]  
**Deciders**: [Who was involved]

#### Context
[What is the issue that we're seeing that is motivating this decision?]

#### Decision
[What is the change that we're proposing and/or doing?]

#### Alternatives Considered
1. [Alternative 1] - [Why not chosen]
2. [Alternative 2] - [Why not chosen]

#### Consequences
**Positive**:
- [Benefit 1]
- [Benefit 2]

**Negative**:
- [Trade-off 1]
- [Trade-off 2]

**Neutral**:
- [Side effect that's neither good nor bad]

#### Related Decisions
- [ADR-X]: [How related]
```

---

## 🔍 DECISIONS

### ADR-005: Worker Handoff Prompts Generated Just-in-Time

**Date**: 2026-05-12
**Status**: Accepted
**Deciders**: SUPERVISOR (subject to user override)

#### Context
The mission is to execute Phases 1–5 (33 tasks). Each handoff prompt embeds context like "what's been done so far", "git sha at handoff", and "your dependencies are met because…". Pre-generating all 33 prompts up front would (a) bloat the supervisor session immediately, (b) guarantee that prompts go stale before workers read them.

#### Decision
- Maintain a permanent `handoff-prompts/_template.md` and `handoff-prompts/README.md`.
- Pre-generate **only** the prompt for the immediately-unblocked task.
- Generate the next batch of prompts at the moment a dependency resolves (e.g., when P1-T01 is accepted, generate P1-T02/T04/T05/T07 in the same session).

#### Alternatives Considered
1. **Pre-generate all 33** — rejected: stale-prompt risk, supervisor context bloat.
2. **No prompts; workers just read the task file directly** — rejected: loses the framework context, "what's been done", and supervision contract.

#### Consequences
**Positive**: prompts are always current; supervisor context stays small; pattern matches the framework's worker-context-isolation intent.
**Negative**: each phase advancement has a small SUPERVISOR overhead to generate the next prompts.

---

### ADR-004: Phase Gates as Checked-in Markdown with Executable Commands

**Date**: 2026-05-12
**Status**: Accepted
**Deciders**: SUPERVISOR (subject to user override)

#### Context
The user requested verification & testing between phases. The framework needs a clear, repeatable contract for when a phase is "done" and the next phase can open.

#### Decision
- Create `docs/SUPERVISOR/phase-gates/phase-{1..5}-gate.md`, one per phase boundary.
- Each gate doc contains: pre-conditions (task list), automated verification (shell commands with expected outputs), manual verification (observable behaviors), promotion checklist, and an append-only run log.
- A phase is **closed** only when its gate passes per `EXECUTION-PLAN.md` §2.2.

#### Alternatives Considered
1. **CI-only gates (e.g., `tools/phase-gate.sh`)** — rejected for now: the gates evolve with the project; markdown is easier to amend in PRs and can be promoted to scripts later.
2. **Implicit gates (just check the phase README acceptance criteria)** — rejected: not actionable enough; no run log.

#### Consequences
**Positive**: anyone can verify a phase from a clean clone by running the listed commands; PR reviewers can spot gate-coverage gaps.
**Negative**: the gate spec must be kept in sync with the plan when task scope changes (mitigated by version-stamping each gate doc).

---

### ADR-001: Adopt AI Supervisor-Worker Framework

**Date**: 2026-05-12 
**Status**: Accepted 
**Deciders**: Project Team

#### Context
Need a structured approach to AI-assisted development that enables:
- Clear task delegation
- Quality assurance
- Knowledge preservation
- Session continuity

SlotSmart is a complex SaaS platform with multiple components (backend API, frontend, database, authentication, multi-tenancy) that benefits from organized AI coordination.

#### Decision
Adopt the Supervisor-Worker Framework for AI collaboration, with framework files stored in `docs/SUPERVISOR/`.

#### Alternatives Considered
1. **Ad-hoc AI usage** - No structure, just ask questions as needed
   - Rejected: Leads to inconsistent results, poor handovers on a complex project
2. **Custom framework** - Build our own from scratch
   - Rejected: Time investment, reinventing the wheel
3. **No AI assistance** - Traditional development only
   - Rejected: Miss productivity benefits

#### Consequences
**Positive**:
- Consistent AI collaboration patterns
- Clear accountability for tasks
- Preserved decision history
- Methodology skills (TDD, debugging, etc.) available as project-level Cursor skills

**Negative**:
- Some overhead in maintaining documents
- Learning curve for the framework

---

### ADR-002: Use ASP.NET Core (.NET 10) with Clean Architecture

**Date**: 2026-05-12 
**Status**: Accepted (from project description) 
**Deciders**: Project Team

#### Context
Backend technology choice for SlotSmart SaaS platform.

#### Decision
Use ASP.NET Core (.NET 10) with Clean Architecture principles, Entity Framework Core, and PostgreSQL/SQL Server support.

#### Alternatives Considered
1. **Node.js / Express** - JavaScript ecosystem
   - Rejected: Team preference for .NET ecosystem
2. **FastAPI (Python)** - Python ecosystem
   - Rejected: Team preference for .NET ecosystem

#### Consequences
**Positive**:
- Strong typing with C#
- Mature ecosystem for enterprise SaaS
- EF Core handles DB abstraction well
- Clean Architecture enables testability and maintainability

**Negative**:
- Higher initial setup complexity vs simpler frameworks
- Clean Architecture requires discipline to maintain boundaries

---

### ADR-003: Use UUIDv7 Identifiers

**Date**: 2026-05-12 
**Status**: Accepted (from project description) 
**Deciders**: Project Team

#### Context
Entity identifier strategy for all domain entities.

#### Decision
Use UUIDv7 (time-ordered UUID) for all entity identifiers.

#### Consequences
**Positive**:
- Time-ordered for database index efficiency
- Globally unique without central coordination
- Supports multi-tenant scenarios well

**Negative**:
- Larger than integer IDs (16 bytes vs 4 bytes)
- Requires UUID generation library

---

## 📊 DECISION CATEGORIES

### Architecture
Decisions about system structure, components, and their relationships.

### Technology Choices
Decisions about frameworks, libraries, and tools.

### API Design
Decisions about interfaces, contracts, and protocols.

### Data Model
Decisions about data structures, storage, and relationships.

### Integration
Decisions about how components and systems connect.

### Security
Decisions about authentication, authorization, and data protection.

### Performance
Decisions about optimization, caching, and scaling.

---

## 🏷️ DECISION INDEX

Quick reference to find decisions by topic:

| ADR | Title | Status | Category |
|-----|-------|--------|----------|
| 001 | Adopt AI Framework | Accepted | Process |
| 002 | ASP.NET Core + Clean Architecture | Accepted | Architecture |
| 003 | UUIDv7 Identifiers | Accepted | Data Model |
| 004 | Phase Gates as Checked-in Markdown with Executable Commands | Accepted | Process |
| 005 | Worker Handoff Prompts Generated Just-in-Time | Accepted | Process |

> ADR-006 is **reserved** for the .NET-version pin decision (open as risk R1 in `EXECUTION-PLAN.md`). It will be authored by the worker executing P1-T01 once the user confirms the pin policy.

---

**Log Version**: 1.1
**Started**: 2026-05-12
**Next ADR Number**: 006 (005 reserved per above)
