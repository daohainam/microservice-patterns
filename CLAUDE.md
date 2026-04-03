# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A collection of focused, production-inspired demos showing how to implement complex microservice patterns on **.NET 10** using **.NET Aspire 13**. The goal is pattern clarity over domain realism — keep it teachable and minimal.

## Build & run

```bash
dotnet restore
dotnet build

# Run all services via Aspire orchestration
dotnet run --project MicroservicePatterns.AppHost

# Run tests
dotnet test tests/IntegrationTests/IntegrationTests.csproj
dotnet test tests/Saga.UnitTests/Saga.UnitTests.csproj

# Run a single test
dotnet test tests/Saga.UnitTests/Saga.UnitTests.csproj --filter "FullyQualifiedName~MyTestName"
```

**Prerequisites:** .NET SDK (10.0+), .NET Aspire workloads, Docker (for PostgreSQL, Kafka, Redis, etc. — all managed by Aspire).

## Solution structure

- `MicroservicePatterns.AppHost/` — Aspire AppHost: orchestrates all services, provisions containers (PostgreSQL, Kafka, Redis, Prometheus, Grafana, OpenTelemetry Collector)
- `patterns/` — One folder per pattern (see below)
- `EventBus/`, `EventBus.Kafka/` — Integration event publishing infrastructure
- `Mediator/` — Custom mediator (replaces MediatR after its license change); supports commands, queries, and pipeline behaviors
- `MicroservicePatterns.ServiceDefaults/` — Common OpenTelemetry, health check, and service discovery wiring applied to all services
- `MicroservicePatterns.Shared/` — Shared abstractions used across patterns
- `MicroservicePatterns.DatabaseMigrationHelpers/` — EF Core migration helpers
- `mcp/` — Model Context Protocol servers exposing pattern services as LLM tools
- `tests/` — Integration and unit tests

## Patterns implemented

| Pattern | Directory | Key services |
|---|---|---|
| CQRS | `patterns/CQRS` | BookService, BorrowerService, BorrowingService, BorrowingHistoryService (read model), Blazor frontend |
| Event Sourcing | `patterns/EventSourcing` | AccountService (banking), NotificationService |
| Saga – Orchestration | `patterns/Saga/TripPlanner` | Central orchestrator coordinates steps |
| Saga – Choreography | `patterns/Saga/OnlineStore` | Services react to events without a central coordinator |
| Transactional Outbox | `patterns/TransactionalOutbox` | Polling + CDC/Debezium variants |
| Webhook / CloudEvents | `patterns/Webhook` | DeliveryService, DispatchService, EventConsumer |
| BFF | `patterns/BFF` | ProductCatalogService, SearchService (Elasticsearch), BackendForPOS |
| Identity | `patterns/Identity` | OpenIddict + ASP.NET Core Identity; Authorization Code + PKCE, Client Credentials, Refresh Token |
| Idempotent Consumer | `patterns/IdempotentConsumer` | CatalogService tracks `callId` header; DB constraint prevents duplicates |
| Resilience | `patterns/Resilience` | Circuit Breaker (Closed → Open → Half-Open) |

## Architecture conventions

**Typical service layout:**
```
[Pattern]/[ServiceName]/
├── Program.cs               # DI registration and Aspire integration
├── Api/ or Endpoints/       # Minimal API route handlers
├── Services/ or Handlers/   # Business logic, command/query handlers
├── Models/ or Dtos/         # Domain models and data transfer objects
├── Infrastructure/          # EF DbContext, event bus adapters
└── Migrations/              # EF Core migrations
```

**Aspire wiring:** All services call `builder.AddServiceDefaults()` (from `MicroservicePatterns.ServiceDefaults`) which configures OpenTelemetry, health checks, and service discovery. External resources (databases, Kafka, Redis) are declared in `AppHost/Program.cs` and injected via Aspire resource references.

**Event bus:** Services publish integration events via `IEventBus` (in-memory default) or `IKafkaEventBus` (Kafka). Subscribers implement `IIntegrationEventHandler<T>`.

**Mediator:** Commands implement `ICommand` / `ICommand<TResult>`, queries implement `IQuery<TResult>`. Handlers are registered via DI. Pipeline behaviors wrap handlers for cross-cutting concerns.

**Database:** PostgreSQL everywhere. EF Core with migrations. The Idempotent Consumer pattern uses a unique DB constraint on the processed-message ID rather than application-level locking.

## C# coding style

From `.github/copilot-instructions.md` — follow these in all C# code:

- C# 13 features preferred
- File-scoped namespace declarations; single-line `using` directives
- Newline before opening `{` of any block (`if`, `for`, `using`, `try`, etc.)
- Pattern matching and switch expressions over chains of `if`/`else`
- `nameof(...)` instead of string literals for member names
- Nullable reference types: declare non-nullable by default, check `null` at entry points only; use `is null` / `is not null` (never `== null`)
- XML doc comments on all public APIs; include `<example>` and `<code>` where applicable
- Tests: xUnit SDK v3 + Moq; no "Arrange/Act/Assert" comments; match capitalization style of nearby test files
