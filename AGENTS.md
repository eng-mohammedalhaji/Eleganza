# Eleganza — AI-native engineering contract

This repository is maintained by autonomous coding agents. Treat this file as
the repository contract: every change must be reproducible, validated by
commands, and safe to merge without a human manually repairing generated
state. Do not ask a human to edit source files when the agent can implement and
verify the change itself.

## Product and runtime boundaries

- Eleganza is a multi-vendor dress marketplace.
- The backend is ASP.NET Core on .NET 10 with PostgreSQL.
- The frontend is Next.js and communicates with the backend through HTTP APIs.
- There is exactly one backend and one persistence runtime. Do not introduce a
  second backend, Convex, client-side database, or hidden mock persistence.
- Demo/fallback data may exist only behind an explicit development mode. It must
  never silently replace the configured API in production.

## Architecture rules: Clean Architecture + DDD

```text
Domain       -> entities, value objects, enums, invariants; no framework or IO
Application  -> use cases, ports, DTOs, authorization decisions, transactions
Infrastructure -> EF Core, PostgreSQL, HTTP providers, encryption, outbox workers
API          -> transport mapping, authentication policy, ProblemDetails only
Web          -> presentation and API client; never pricing or inventory truth
```

- Keep dependencies pointing inward. Domain must remain framework-independent.
- Model business state transitions inside aggregates; do not mutate status from
  controllers or persistence code.
- Application services own use-case orchestration and must calculate totals,
  stock reservations, ownership, and idempotency from server-side data.
- Infrastructure integrations are adapters behind interfaces. Vanex-specific
  field names belong in the Vanex adapter, not in Domain or UI.
- Controllers stay thin: validate transport shape, call a use case, return a
  contract. Do not put business rules in controllers or React components.
- Use migrations for every EF model change. Never modify a production schema by
  hand or delete migrations to make a build pass.

## Security and data consistency

- Never commit or log passwords, access tokens, cookies, connection strings, or
  personal delivery data. Vendor Vanex tokens must be encrypted at rest and
  must never be returned to the browser.
- Configuration, URLs, provider paths, limits, fees, feature flags, and external
  identifiers come from typed options/environment. Do not hardcode business
  configuration, city IDs, credentials, or provider defaults in C# or React.
- Use allowlisted CORS origins, secure cookie settings in production, rate
  limiting on authentication and sensitive public endpoints, and least-privilege
  authorization policies.
- Local order creation is the source of truth. Calculate price and stock on the
  server, reserve inventory transactionally, use idempotency keys with request
  fingerprints, and use the outbox for external side effects.
- Treat external calls as at-least-once delivery. Use bounded retries,
  timeouts, persisted attempts, dead-letter handling, and idempotent response
  processing. Never lose the local order because a provider is unavailable.
- Do not expose stack traces or provider response bodies to clients. Redact
  sensitive values from logs and map failures to stable error codes.

## Errors and contracts

- All API failures use RFC 9457 ProblemDetails with a stable `code`, a safe
  user-facing `detail`, a trace/correlation identifier, and field `errors` when
  validation fails.
- Domain/application exceptions must be translated centrally by the API
  exception handler. Do not catch-and-ignore errors in controllers or UI.
- Frontend code must render server error codes/messages intentionally; never use
  `alert`, raw exception text, or a generic success state for a failed request.
- Public contracts are versionable. Prefer additive changes and update tests,
  frontend types, and docs together.

## AI-agent workflow

1. Inspect the current tree, git diff, configuration, and relevant tests before
   editing. Preserve unrelated user changes.
2. State the invariant being protected and implement the smallest complete
   vertical slice across Domain, Application, Infrastructure, API, and Web.
3. Use `apply_patch` for edits. Do not use shell redirection or generated
   ad-hoc files to modify source.
4. Add or update unit tests for invariants and integration/contract tests for
   HTTP and provider boundaries. External services must be replaced with a
   deterministic test server; never use real credentials in tests.
5. Run formatting/type checks, frontend build, `dotnet build`, unit tests, and
   EF pending-model validation. If PostgreSQL or a provider sandbox is
   unavailable, report that exact unverified gate instead of claiming success.
6. Review the diff for secrets, hardcoded configuration, broken authorization,
   data races, duplicate side effects, and misleading UI fallback behavior.
7. Update `docs/` when a workflow, contract, migration, deployment variable, or
   acceptance criterion changes.

## Required verification commands

```text
pnpm exec tsc --noEmit
pnpm exec next build
dotnet build Eleganza.slnx --no-restore -p:UseAppHost=false
dotnet test Eleganza.slnx --no-restore
dotnet ef migrations has-pending-model-changes --project src/Eleganza.Infrastructure --startup-project src/Eleganza.Api
```

Use the repository's documented temporary `DOTNET_ROOT`/`DOTNET_CLI_HOME`
values when the local environment requires them. Never weaken a test or remove
an invariant merely to obtain a green build.
