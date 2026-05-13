# Solution Structure — Clean Architecture

The repository is a monorepo with a .NET solution under `backend/` and the React app under `frontend/`.

```text
slotsmart/
├── backend/
│   ├── SlotSmart.sln
│   ├── src/
│   │   ├── SlotSmart.Domain/                ← Entities, value objects, domain events, invariants. No deps.
│   │   ├── SlotSmart.Application/           ← Use cases, commands/queries, validators, DTOs. Depends on Domain.
│   │   ├── SlotSmart.Infrastructure/        ← EF Core, repositories, identity, email, jobs. Depends on Application.
│   │   ├── SlotSmart.Api/                   ← ASP.NET Core host, endpoints, OpenAPI, middleware. Composition root.
│   │   └── SlotSmart.Shared/                ← Cross-cutting primitives (Result<T>, errors, paging). Depends on nothing.
│   └── tests/
│       ├── SlotSmart.Domain.Tests/
│       ├── SlotSmart.Application.Tests/
│       ├── SlotSmart.Infrastructure.Tests/  ← Uses Testcontainers (Postgres).
│       └── SlotSmart.Api.Tests/             ← End-to-end via WebApplicationFactory.
├── frontend/
│   ├── package.json
│   ├── src/
│   │   ├── app/                ← App shell, providers, routing, theme.
│   │   ├── features/           ← One folder per feature (auth, clubs, members, training, booking, …).
│   │   │   └── <feature>/
│   │   │       ├── api/        ← Query/mutation hooks built on the generated client.
│   │   │       ├── components/
│   │   │       ├── pages/
│   │   │       └── types.ts
│   │   ├── shared/             ← Reusable UI, hooks, utils. No feature imports.
│   │   ├── lib/                ← Generated API client, i18n, query client, auth helpers.
│   │   └── main.tsx
│   └── tests/                  ← Playwright E2E.
├── docs/
│   ├── project-description.md
│   └── plan/
├── docker/
│   ├── docker-compose.yml          ← postgres + api + web + (later) seq/otel collector
│   ├── api.Dockerfile
│   └── web.Dockerfile
├── .github/workflows/
├── CHANGELOG.md
└── README.md
```

## Layer rules (must enforce in CI)

```text
Domain         depends on → (nothing)
Application    depends on → Domain, Shared
Infrastructure depends on → Application, Domain, Shared
Api            depends on → Application, Infrastructure, Domain, Shared (composition only)
Shared         depends on → (nothing)

Tests can depend on anything; production code cannot reference test assemblies.
```

Use **NetArchTest** (or Roslyn analyzers) in `SlotSmart.Architecture.Tests` to assert these rules.

## Entity base type

Every entity derives from `SlotSmart.Domain.Common.Entity`, which exposes only a public `EntityId : Guid` (UUIDv7). The database primary key is a `bigint` surrogate added as an EF Core **shadow property** in `SlotSmart.Infrastructure/Persistence/Configurations/EntityConfiguration<T>`. The Domain layer never references the surrogate. Foreign keys reference the surrogate `bigint`; domain code expresses relationships via navigation properties. See [`entity-identity.md`](./entity-identity.md) for the canonical pattern.

## Module organization inside Application

Inside `SlotSmart.Application`, group by **feature** then by **operation**:

```text
SlotSmart.Application/
├── Features/
│   ├── Members/
│   │   ├── Commands/
│   │   │   ├── RegisterMember/
│   │   │   │   ├── RegisterMemberCommand.cs
│   │   │   │   ├── RegisterMemberHandler.cs
│   │   │   │   └── RegisterMemberValidator.cs
│   │   │   └── …
│   │   ├── Queries/
│   │   │   ├── GetMemberById/
│   │   │   └── …
│   │   └── Dtos/
│   ├── Clubs/
│   ├── Trainings/
│   └── Bookings/
├── Common/
│   ├── Abstractions/         ← e.g. ITenantContext, ICurrentUser, IClock, IEmailSender
│   ├── Behaviors/            ← MediatR pipeline behaviors: validation, logging, transactions, tenancy.
│   └── Errors/
└── DependencyInjection.cs
```

## Module organization inside Infrastructure

```text
SlotSmart.Infrastructure/
├── Persistence/
│   ├── SlotSmartDbContext.cs
│   ├── Configurations/       ← IEntityTypeConfiguration<T> per aggregate.
│   ├── Migrations/
│   ├── Repositories/         ← Only for aggregates that need explicit repos; otherwise use DbContext directly.
│   └── Interceptors/         ← Auditing, tenant filter, outbox dispatch.
├── Identity/                 ← OpenIddict configuration, user store, password hashing.
├── Tenancy/                  ← ITenantResolver, ITenantContext implementation.
├── Email/
├── Jobs/                     ← Quartz job definitions.
├── Outbox/                   ← Outbox pattern for reliable events.
└── DependencyInjection.cs
```

## API project organization

```text
SlotSmart.Api/
├── Endpoints/                ← Minimal API endpoint groups, one file per feature.
├── Middleware/               ← Exception handler, tenant resolution, request logging.
├── OpenApi/                  ← Document filters, examples, security schemes.
├── Program.cs
└── appsettings.json
```

Each endpoint group exposes a static `Map<Feature>Endpoints(this IEndpointRouteBuilder app)` method that the composition root wires up. Endpoints are thin: they map HTTP → command/query → result.

## Result pattern

Avoid throwing for expected failures. Return `Result<T>` from handlers with typed `Error` values; the API layer translates `Error` → HTTP problem details. Throw only for unexpected/infrastructure failures.

## Source generators / analyzers

- Enable `TreatWarningsAsErrors` for all production projects.
- Enable nullable reference types project-wide.
- Use `Microsoft.CodeAnalysis.NetAnalyzers` and `StyleCop.Analyzers` with a curated ruleset.
- Use Roslynator suggestions in CI.
