# Task `P1-T07` — Observability Baseline (Serilog + OpenTelemetry + Seq)

> **Phase**: 1 — Foundation
> **Estimated size**: S
> **Depends on**: P1-T01
> **Can run in parallel with**: P1-T02, P1-T04, P1-T05, P1-T06

---

## 1. Context

We want **structured** logs and **traces** from day one so debugging is easy as soon as real features land. Locally we ship logs and traces to a single tool — **Seq** — which is added to `docker-compose`.

## 2. Goal

> Every API request emits a structured log line with `traceId`, `tenantId` (null for now), `userId` (null), `requestPath`, and duration; in dev, those lines + traces show up in Seq at `http://localhost:5341`.

## 3. Scope

### In scope

- Add Serilog with `Serilog.AspNetCore` and sinks: `Console` + `Seq`.
- Add OpenTelemetry with `OpenTelemetry.Extensions.Hosting`, ASP.NET Core instrumentation, EF Core instrumentation, HttpClient instrumentation; exporter: OTLP to Seq.
- Configure log enrichers for `TraceId`, `SpanId`, `Environment`, `Application`.
- Add a placeholder enricher hook for `TenantId` and `UserId` (returns null today; populated in Phase 2).
- Add Seq as a service to `docker-compose.yml` (depends on P1-T03 — if P1-T03 isn't done yet, just add the env vars and a snippet to be appended).
- Health endpoint logs at `Debug` only; happy-path logging is light.
- Add a `RequestLoggingOptions` configuration block in `appsettings` to allow tuning log verbosity per environment.

### Out of scope

- Metrics dashboards / Grafana → V2.
- Centralized cloud sink → CD task.
- Frontend telemetry (Sentry / RUM) → later, after Phase 5.

## 4. Inputs

- Architecture docs:
  - [`../00-architecture/tech-stack.md`](../00-architecture/tech-stack.md) (logging + telemetry section)
  - [`../00-architecture/coding-standards.md`](../00-architecture/coding-standards.md) (errors and logging section)
- Env vars introduced:
  - `Seq__ServerUrl` (default `http://seq:5341` inside compose, `http://localhost:5341` outside)
  - `Otel__ServiceName=slotsmart-api`

## 5. Deliverables

- Add NuGet packages via CPM:
  - `Serilog.AspNetCore`, `Serilog.Sinks.Seq`, `Serilog.Enrichers.Environment`, `Serilog.Enrichers.Thread`
  - `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`.
- `backend/src/SlotSmart.Api/Telemetry/SerilogConfig.cs`
- `backend/src/SlotSmart.Api/Telemetry/OpenTelemetryConfig.cs`
- Update `Program.cs` to use Serilog + OTel.
- Add Seq service to `docker/docker-compose.yml`.
- `CHANGELOG.md` updated.

## 6. Acceptance Criteria

- [ ] `make up` starts Seq at `http://localhost:5341`.
- [ ] Hitting `/api/v1/health` produces a structured log line in Seq with fields `RequestPath`, `StatusCode`, `Elapsed`, `TraceId`.
- [ ] A trace is visible in Seq's tracing view showing the ASP.NET Core span; if a DB call had happened, the EF span would nest under it.
- [ ] Logs printed to console are JSON in Production, but human-readable in Development.
- [ ] No PII is logged (manually verified by reviewing what's emitted on the health path).

## 7. Definition of Done

- [ ] All Acceptance Criteria boxes ticked.
- [ ] Log levels per environment configurable in `appsettings`.
- [ ] CHANGELOG.md updated.

## 8. Handoff notes / gotchas

- Use `builder.Host.UseSerilog(...)` so Microsoft logs are also captured.
- Configure `AddOpenTelemetry().WithTracing(...)` with the OTLP exporter pointing to Seq's OTLP endpoint (port `5341` HTTP for ingestion, depending on Seq version configure accordingly — verify with the docs).
- Don't enable EF query logging at `Information` in non-dev environments; queries can leak PII.
- The `TenantId` and `UserId` enrichers are set up here but **read from a placeholder** until Phase 2 wires `ITenantContext` and `ICurrentUser`. Make the enrichers tolerant of null.

## 9. Suggested execution outline

1. Add Serilog packages and configure in `Program.cs` (bootstrap logger first, then full config).
2. Add OTel packages and configure tracing with OTLP exporter.
3. Add Seq to compose; document the URL.
4. Add enrichers (incl. placeholder tenant/user enrichers).
5. Hit `/api/v1/health` a few times and verify Seq shows logs + traces.
6. Update CHANGELOG.

## 10. Open questions / risks

- Question: Seq vs Aspire dashboard for local dev? **Decision**: Seq — single tool for logs + traces, great UI, free tier covers our usage.
- Risk: extra container adds RAM cost on small dev laptops. **Mitigation**: Seq is optional in compose via a profile (`docker compose --profile observability up`).
