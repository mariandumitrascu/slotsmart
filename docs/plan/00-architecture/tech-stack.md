# Tech Stack — Canonical Choices

> Any deviation from this document must be discussed with the project owner first.

## Backend

| Concern | Choice | Notes |
|---|---|---|
| Language / runtime | C# 13 on **.NET 10** | LTS path; use latest preview SDK if .NET 10 GA not yet shipped, then upgrade. |
| Web framework | **ASP.NET Core** Minimal APIs + Controllers (where helpful) | Prefer Minimal APIs for new endpoints; use controllers only for cross-cutting concerns. |
| ORM | **Entity Framework Core 10** | Code-first migrations. No EF Power Tools required. |
| Primary database | **PostgreSQL 16+** for local & cloud | SQL Server compatibility is a goal but **Postgres is the default** target. |
| Identity / auth | **JWT bearer** access tokens, refresh tokens. **OpenIddict** to host the OAuth2/OIDC endpoints. | We host our own authorization server. Plan for external IdP federation later. |
| Validation | **FluentValidation** | One validator per command/request. |
| Mapping | **Mapperly** (source-generated) or hand-written | No AutoMapper. |
| Mediator | **MediatR** (or hand-rolled command bus) | Decision deferred until Phase 2. Default: MediatR. |
| Logging | **Serilog** → console + structured sink (Seq / OTel) | Use `Microsoft.Extensions.Logging` abstractions in code. |
| Telemetry | **OpenTelemetry** for traces + metrics | Exporter configurable (OTLP). |
| Background jobs | **Quartz.NET** or **Hangfire**; default **Quartz.NET** | Needed from Phase 4 (recurring trainings) and Phase 6 (notifications). |
| Email | **Fluent Email** + SMTP provider abstraction | Concrete provider chosen in Phase 6. |
| Testing | **xUnit**, **FluentAssertions**, **Testcontainers** for integration tests | Use Postgres testcontainers. |

## Frontend

| Concern | Choice | Notes |
|---|---|---|
| Framework | **React 19** + **TypeScript 5.x** | Strict mode on. |
| Build tooling | **Vite** | No CRA, no Next.js (yet). |
| UI library | **Material UI v6+** | Default theme customized for SlotSmart brand later. |
| State / data | **TanStack Query** for server state, **Zustand** for tiny client state | No Redux. |
| Routing | **React Router v6+** | Data routers. |
| Forms | **React Hook Form** + **Zod** | One schema per form. |
| API client | Generated client from **OpenAPI** spec via `openapi-typescript` or `orval` | Single source of truth = backend OpenAPI. |
| Testing | **Vitest** + **React Testing Library**; **Playwright** for E2E | E2E added late Phase 1. |
| i18n | **react-i18next** | English first; structure ready for translation. |

## Identifiers

- All entity IDs are **UUIDv7** generated server-side.
- Store as `uuid` in Postgres, `Guid` in C#.
- Never expose internal numeric IDs.

## Infrastructure / DevOps

| Concern | Choice | Notes |
|---|---|---|
| Containers | **Docker** + **docker-compose** for local dev | Multi-stage Dockerfiles. |
| CI | **GitHub Actions** | Build, test, lint, container build on PR. |
| CD | TBD per environment | Not part of MVP scope; CI must be cloud-ready. |
| Secrets (local) | `.env` files + `dotnet user-secrets` | Never commit secrets. |
| Secrets (cloud) | Cloud-native secret store (Azure Key Vault / AWS SM) — TBD | Decided when picking the host. |
| Observability | OTel → vendor of choice; locally → Aspire dashboard or Seq + Jaeger | Decision in Phase 1. |

## Versioning & dependencies

- Lock major versions in `Directory.Packages.props` (Central Package Management) for .NET.
- Lock npm versions via `package-lock.json` committed.
- Renovate / Dependabot configured early.
