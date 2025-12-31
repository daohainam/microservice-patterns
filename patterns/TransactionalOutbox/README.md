# Transactional Outbox Pattern Implementation

## Overview

This demo shows how to implement the **Transactional Outbox** pattern to ensure reliable event publishing in a microservices architecture.

## Problem Statement

How do you ensure that when a service updates its database **and** publishes an integration event, **either both happen or neither do**?

The classic problem:
```csharp
// ❌ This can fail!
await dbContext.SaveChangesAsync();      // ✓ Succeeds
await eventPublisher.PublishAsync(evt);  // ✗ Fails - event is lost!
```

## Solution

The Transactional Outbox pattern stores events in the same database transaction as the business data:

1. Business data changes and events are written to the database **in the same transaction**
2. A background process reads unpublished events from the **outbox table**
3. Events are published to the message bus
4. Published events are marked as processed

This ensures **at-least-once delivery** semantics.

## Implementations

### Polling-Based Outbox
A background worker polls the outbox table for unpublished messages.

**Pros:**
- Simple to implement
- Works with any database
- Easy to understand

**Cons:**
- Polling interval introduces latency
- Database overhead from constant polling

### Log Tailing (CDC)
Reads the database transaction log to detect new events.

**Pros:**
- Near real-time event publishing
- No polling overhead
- Leverages database built-in features

**Cons:**
- More complex setup
- Requires database-specific implementation (e.g., PostgreSQL logical replication, Debezium)

## Database Schema

### Outbox Table (Polling)

```sql
CREATE TABLE OutboxMessages (
    Id UUID PRIMARY KEY,
    CreationDate TIMESTAMP NOT NULL,
    PayloadType VARCHAR(255) NOT NULL,
    Payload JSONB NOT NULL,
    ProcessedDate TIMESTAMP NULL
);

CREATE INDEX idx_outbox_unprocessed 
ON OutboxMessages(CreationDate) 
WHERE ProcessedDate IS NULL;
```

### Outbox Table (Log Tailing)

```sql
CREATE TABLE OutboxMessagesForLogTailing (
    Id UUID PRIMARY KEY,
    PayloadType VARCHAR(255) NOT NULL,
    Payload JSONB NOT NULL
);
```

## Service Implementation

### AccountService (Banking)
Demonstrates both polling and log tailing approaches.

**Endpoints:**
- `POST /api/outbox/v1/accounts` - Open account
- `POST /api/outbox/v1/accounts/{id}/deposit` - Deposit money
- `POST /api/outbox/v1/accounts/{id}/withdraw` - Withdraw money

**Flow with Transactional Outbox:**

```csharp
public async Task<Result> OpenAccountAsync(string accountNumber, string owner, decimal initialBalance)
{
    await using var unitOfWork = new UnitOfWork(connection);
    await unitOfWork.BeginTransactionAsync();
    
    try
    {
        // 1. Save business data
        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Owner = owner,
            Balance = initialBalance
        };
        unitOfWork.AccountDbContext.Accounts.Add(account);
        
        // 2. Save event to outbox (same transaction!)
        var evt = new AccountOpenedIntegrationEvent(account.Id, accountNumber, owner, initialBalance);
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            PayloadType = typeof(AccountOpenedIntegrationEvent).FullName,
            Payload = JsonSerializer.Serialize(evt),
            ProcessedDate = null
        };
        await unitOfWork.OutboxForPollingRepository.AddAsync(outboxMessage);
        
        // 3. Commit transaction - both or neither!
        await unitOfWork.CommitTransactionAsync();
        
        return Result.Success();
    }
    catch (Exception ex)
    {
        await unitOfWork.RollbackTransactionAsync();
        return Result.Failure(ex.Message);
    }
}
```

## Polling Publisher

```csharp
public class PollingPublisher : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Fetch unpublished messages
            var messages = await repository.GetUnprocessedMessagesAsync(batchSize: 20);
            
            foreach (var message in messages)
            {
                // 2. Deserialize event
                var eventType = Type.GetType(message.PayloadType);
                var evt = JsonSerializer.Deserialize(message.Payload, eventType);
                
                // 3. Publish to message bus
                await eventPublisher.PublishAsync(evt);
                
                // 4. Mark as processed
                message.ProcessedDate = DateTime.UtcNow;
                await repository.UpdateAsync(message);
            }
            
            // Wait before next poll
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## Log Tailing with PostgreSQL

### Enable Logical Replication

```sql
-- Enable logical replication in postgresql.conf
ALTER SYSTEM SET wal_level = logical;

-- Create publication
CREATE PUBLICATION outbox_pub FOR TABLE OutboxMessagesForLogTailing;
```

### Listen to Changes

```csharp
public class LogTailingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(stoppingToken);
        
        // Listen to database notifications
        conn.Notification += async (sender, e) =>
        {
            var message = JsonSerializer.Deserialize<OutboxMessage>(e.Payload);
            
            var eventType = Type.GetType(message.PayloadType);
            var evt = JsonSerializer.Deserialize(message.Payload, eventType);
            
            await eventPublisher.PublishAsync(evt);
        };
        
        using var cmd = new NpgsqlCommand("LISTEN outbox_channel", conn);
        await cmd.ExecuteNonQueryAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await conn.WaitAsync(stoppingToken);
        }
    }
}
```

## Integration with Debezium

Debezium can stream changes from the database to Kafka:

```yaml
# docker-compose.yml
services:
  debezium:
    image: debezium/connect:latest
    environment:
      - BOOTSTRAP_SERVERS=kafka:9092
      - GROUP_ID=1
    ports:
      - "8083:8083"
```

```json
// Register PostgreSQL connector
POST http://localhost:8083/connectors
{
  "name": "outbox-connector",
  "config": {
    "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
    "database.hostname": "postgres",
    "database.port": "5432",
    "database.user": "postgres",
    "database.password": "postgres",
    "database.dbname": "outbox",
    "table.include.list": "public.outboxmessagesforlogtailing",
    "transforms": "outbox",
    "transforms.outbox.type": "io.debezium.transforms.outbox.EventRouter"
  }
}
```

## At-Least-Once Delivery

The Transactional Outbox pattern guarantees **at-least-once delivery**:

- Events may be published multiple times
- Consumer services must be **idempotent**
- Use idempotency keys or deduplication to handle duplicates

```csharp
// Consumer side: Check if event was already processed
public async Task HandleAsync(AccountOpenedIntegrationEvent evt)
{
    if (await processedEventRepository.ExistsAsync(evt.EventId))
    {
        _logger.LogInformation("Event {EventId} already processed, skipping", evt.EventId);
        return;
    }
    
    // Process event
    await ProcessEventAsync(evt);
    
    // Record that we processed it
    await processedEventRepository.AddAsync(evt.EventId);
}
```

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Open an account
curl -X POST http://localhost:5040/api/outbox/v1/accounts \
  -H "Content-Type: application/json" \
  -d '{"accountNumber":"ACC001","owner":"John Doe","initialBalance":1000}'

# Watch the logs - you'll see:
# 1. Account created and outbox message inserted (same transaction)
# 2. Polling publisher picks up the message
# 3. Event is published to the event bus
# 4. Outbox message is marked as processed
```

## Key Benefits

- **Reliability**: No lost events
- **Consistency**: Database and events always in sync
- **Atomicity**: Single transaction guarantees
- **Auditability**: Outbox table provides event history

## Trade-offs

**Polling:**
- ✓ Simple
- ✓ Works everywhere
- ✗ Introduces latency
- ✗ Polling overhead

**Log Tailing:**
- ✓ Near real-time
- ✓ No polling overhead
- ✗ Complex setup
- ✗ Database-specific

## Common Pitfalls

1. **Forgetting to delete processed messages**: Outbox can grow indefinitely
2. **Not handling duplicates**: Consumers must be idempotent
3. **Long-running transactions**: Keep transactions short
4. **Polling too frequently**: Balance latency vs database load

## Cleanup Strategy

```csharp
// Delete processed messages older than 30 days
public async Task CleanupAsync()
{
    var cutoffDate = DateTime.UtcNow.AddDays(-30);
    await dbContext.OutboxMessages
        .Where(m => m.ProcessedDate != null && m.ProcessedDate < cutoffDate)
        .ExecuteDeleteAsync();
}
```

## Learn More

Watch the detailed video explanation: [Transactional Outbox Pattern Deep Dive](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez)
