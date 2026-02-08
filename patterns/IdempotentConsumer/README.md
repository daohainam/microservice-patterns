# Idempotent Consumer Pattern Implementation

## Overview

This demo shows how to implement the **Idempotent Consumer** pattern in a microservices architecture. This pattern ensures that duplicate messages or requests are processed only once, even if they arrive multiple times.

## Problem Statement

In distributed systems, messages can be delivered more than once due to network retries, client retries, or system failures. How do you ensure that processing the same message or request multiple times doesn't corrupt your data or cause unintended side effects?

## Solution

The Idempotent Consumer pattern tracks which messages have already been processed using a unique identifier (`callId`). Before processing a request:

1. Check if the `callId` has already been processed
2. If yes, return success without reprocessing
3. If no, process the request and record the `callId` atomically in the same transaction

This implementation uses a database-backed approach with PostgreSQL unique constraints to handle race conditions safely.

## Service

### CatalogService

Manages a product catalog with idempotent create and update operations.

**Endpoints:**

- `GET /api/idempotentconsumer/v1/products` - List all products
- `GET /api/idempotentconsumer/v1/products/{id}` - Get a specific product
- `POST /api/idempotentconsumer/v1/products` - Create a product (idempotent via `callId` header)
- `PUT /api/idempotentconsumer/v1/products/{id}` - Update a product (idempotent via `callId` header)

**Required Header:**
- `callId` (GUID) - Unique identifier for the request/message

## How It Works

### 1. Idempotency Key
Every write operation requires a unique `callId` in the request header. This acts as the idempotency key.

### 2. Processed Messages Table
```sql
CREATE TABLE ProcessedMessages (
    Id UUID PRIMARY KEY,           -- The callId
    ProcessedAtUtc TIMESTAMP NOT NULL
);
```

### 3. Processing Flow

```
Client Request → Check if callId exists → Yes → Return success
                          ↓ No
                  Begin Transaction
                          ↓
              Add callId to ProcessedMessages
                          ↓
              Process business logic
                          ↓
                  Commit Transaction
                          ↓
                  Return success
```

### 4. Race Condition Handling

When concurrent requests arrive with the same `callId`, the database unique constraint prevents duplicates:

```csharp
try 
{
    context.ProcessedMessages.Add(new ProcessedMessage { Id = callId });
    context.Products.Add(product);
    await context.SaveChangesAsync();
    return Ok(product);
}
catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
{
    // Concurrent request already processed this callId
    return Ok(product);
}
```

## Key Benefits

- **Guaranteed Idempotency**: Same request processed only once, even under high concurrency
- **Data Integrity**: Atomic operations prevent partial updates
- **Simple Implementation**: Uses standard database features (unique constraints)
- **Safe Retries**: Clients can safely retry requests without fear of duplication

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Create a product (first time)
curl -X POST http://localhost:5000/api/idempotentconsumer/v1/products \
  -H "Content-Type: application/json" \
  -H "callId: 12345678-1234-1234-1234-123456789012" \
  -d '{"name":"Laptop","description":"Gaming laptop","price":1299.99}'

# Retry the same request (idempotent - no duplicate created)
curl -X POST http://localhost:5000/api/idempotentconsumer/v1/products \
  -H "Content-Type: application/json" \
  -H "callId: 12345678-1234-1234-1234-123456789012" \
  -d '{"name":"Laptop","description":"Gaming laptop","price":1299.99}'

# List products (should show only one product)
curl http://localhost:5000/api/idempotentconsumer/v1/products
```

## When to Use

✅ **Use this pattern when:**
- Processing messages from message queues (Kafka, RabbitMQ, Azure Service Bus)
- Building APIs that clients might retry
- Implementing exactly-once semantics in distributed systems
- Handling webhook deliveries
- Processing payment transactions

❌ **Avoid this pattern when:**
- Operations are naturally idempotent (e.g., updating a value to a specific state)
- Performance overhead of tracking processed IDs is unacceptable
- You have a simpler mechanism (like unique business keys)

## Trade-offs

### Pros
- ✅ Prevents duplicate processing
- ✅ Simple to understand and implement
- ✅ Works with any database supporting unique constraints
- ✅ Handles race conditions automatically

### Cons
- ❌ Requires storing processed message IDs (storage overhead)
- ❌ Need to periodically clean up old processed IDs
- ❌ Requires clients to generate unique IDs
- ❌ Slight performance overhead per request

## Common Pitfalls

1. **Forgetting to include callId in the same transaction**
   - Always insert the processed message ID and perform business logic in the same transaction

2. **Not handling duplicate key exceptions**
   - Concurrent requests will cause database constraint violations - handle them gracefully

3. **Using sequential IDs**
   - Use GUIDs to avoid collisions across distributed systems

4. **Never cleaning up old IDs**
   - Implement a cleanup job to remove old processed messages (e.g., older than 30 days)

5. **Reusing the same callId for different operations**
   - Each unique request must have its own unique callId

## Learn More

For a detailed explanation of this pattern, watch the [Microservice Patterns playlist](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez).

## Related Patterns

- **Transactional Outbox**: Often used together to ensure both processing and event publishing are idempotent
- **Event Sourcing**: Events naturally have unique IDs, making them idempotent
- **Saga**: Saga steps should be idempotent to handle retries safely
