# Event Sourcing Pattern Implementation

## Overview

This demo shows how to implement **Event Sourcing** in a microservices architecture using .NET Aspire.

## Problem Statement

How do you model complex aggregates while keeping updates atomic, auditable, and easy to reason about? How do you maintain a full history of an entity's lifecycle?

## Solution

Event Sourcing stores all changes to application state as a sequence of events:

- State changes are represented as **events** instead of direct database updates
- Current state is rebuilt by **replaying the event stream**
- Every change is preserved in the **event store** for audit, debugging, and analytics
- Complex updates become sequences of **small atomic events**

## Services

### AccountService (Banking)
Manages bank accounts using event sourcing.

**Endpoints:**
- `POST /api/eventsourcing/v1/accounts` - Open a new account
- `POST /api/eventsourcing/v1/accounts/{id}/deposit` - Deposit money
- `POST /api/eventsourcing/v1/accounts/{id}/withdraw` - Withdraw money
- `GET /api/eventsourcing/v1/accounts/{id}` - Get account current state

### NotificationService
Consumes account events via PostgreSQL LISTEN/NOTIFY to send notifications.

## Domain Events

### AccountOpenedEvent
Emitted when a new account is created.

```csharp
public record AccountOpenedEvent(
    Guid AccountId,
    string AccountNumber,
    string Owner,
    decimal InitialBalance
) : IDomainEvent;
```

### BalanceChangedEvent
Emitted when money is deposited or withdrawn.

```csharp
public record BalanceChangedEvent(
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string TransactionType
) : IDomainEvent;
```

## Event Store

Events are stored in PostgreSQL using the following structure:

```sql
CREATE TABLE EventStreams (
    StreamId UUID PRIMARY KEY,
    Version BIGINT NOT NULL
);

CREATE TABLE Events (
    Id UUID PRIMARY KEY,
    StreamId UUID NOT NULL,
    Type VARCHAR(255) NOT NULL,
    Data JSONB NOT NULL,
    CreatedAtUtc TIMESTAMP NOT NULL
);
```

## Flow

1. Client calls `POST /accounts` → `AccountOpenedEvent` persisted
2. Client calls `POST /accounts/{id}/deposit` → `BalanceChangedEvent` persisted
3. To get current state: Load all events for the account and replay them
4. NotificationService listens to the event stream via PostgreSQL NOTIFY
5. When events are added, NotificationService logs them (in real app: send email/SMS)

## Key Benefits

- **Full Audit Trail**: Every change is permanently recorded
- **Temporal Queries**: Can reconstruct state at any point in time
- **Event Replay**: Can rebuild state from events if needed
- **Debugging**: Easy to trace what happened and when
- **Domain Insight**: Events represent business-meaningful actions

## Aggregate Implementation

```csharp
public class Account : Aggregate
{
    public string AccountNumber { get; private set; }
    public string Owner { get; private set; }
    public decimal Balance { get; private set; }

    public void Open(string accountNumber, string owner, decimal initialBalance)
    {
        ApplyChange(new AccountOpenedEvent(Id, accountNumber, owner, initialBalance));
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive");

        ApplyChange(new BalanceChangedEvent(Id, amount, Balance + amount, "Deposit"));
    }

    // Apply methods handle state changes
    private void Apply(AccountOpenedEvent evt)
    {
        AccountNumber = evt.AccountNumber;
        Owner = evt.Owner;
        Balance = evt.InitialBalance;
    }

    private void Apply(BalanceChangedEvent evt)
    {
        Balance = evt.BalanceAfter;
    }
}
```

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Open an account
curl -X POST http://localhost:5010/api/eventsourcing/v1/accounts \
  -H "Content-Type: application/json" \
  -d '{"accountNumber":"ACC001","owner":"John Doe","initialBalance":1000}'

# Deposit money
curl -X POST http://localhost:5010/api/eventsourcing/v1/accounts/{id}/deposit \
  -H "Content-Type: application/json" \
  -d '{"amount":500}'

# Get current state (rebuilt from events)
curl http://localhost:5010/api/eventsourcing/v1/accounts/{id}
```

## Event Versioning

When domain events change over time, use event versioning strategies:

1. **Event Transformation**: Transform old events to new format on read
2. **Multiple Handlers**: Support multiple event versions in the same codebase
3. **Event Migration**: Run a migration to convert old events

## Common Pitfalls

1. **Forgetting to increment version**: Always increment the aggregate version after applying events
2. **Modifying past events**: Never change historical events; create new compensating events
3. **Large aggregates**: Keep aggregates focused; don't try to event-source everything
4. **Not handling concurrency**: Use optimistic concurrency with version checks

## Learn More

Watch the detailed video explanation: [Event Sourcing Pattern Deep Dive](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez)
