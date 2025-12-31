# Saga Pattern Implementation

## Overview

This demo shows how to implement **distributed transactions** across microservices using the **Saga pattern** in both orchestration and choreography styles.

## Problem Statement

How do you implement a business transaction spanning multiple microservices without using distributed transactions (2PC)?

## Solution

A Saga is a sequence of local transactions where each service:
1. Performs its local transaction
2. Publishes an event or sends a message
3. The next service in the sequence is triggered

If any step fails, **compensating transactions** are executed to undo previous steps.

## Implementations

### Orchestration-Based Saga (Trip Planner)
A central orchestrator coordinates the saga steps.

**Services:**
- **TripPlanningService** (Orchestrator) - Coordinates the booking process
- **HotelService** - Manages hotel bookings
- **TicketService** - Manages flight tickets
- **PaymentService** - Processes payments

**Flow:**
1. Client creates a trip booking
2. TripPlanningService reserves hotel
3. TripPlanningService reserves ticket
4. TripPlanningService processes payment
5. If any step fails, compensating actions are triggered in reverse order

**Endpoints:**
```
POST /api/saga/v1/trips - Create a trip booking
GET /api/saga/v1/trips/{id} - Get trip status
```

### Choreography-Based Saga (Online Store)
Services react to events without a central coordinator.

**Services:**
- **OrderService** - Creates and manages orders
- **InventoryService** - Manages product inventory
- **PaymentService** - Processes payments
- **CatalogService** - Manages product catalog

**Flow:**
1. OrderService creates order → `OrderCreatedEvent`
2. InventoryService reserves items → `ItemsReservedEvent` or `ItemsReservationFailedEvent`
3. PaymentService processes payment → `PaymentProcessedEvent` or `PaymentFailedEvent`
4. If reservation or payment fails, compensating events trigger rollback

**Endpoints:**
```
POST /api/saga/v1/orders - Create an order
GET /api/saga/v1/orders/{id} - Get order status
```

## Orchestration vs Choreography

### Orchestration
**Pros:**
- Clear, centralized flow
- Easier to understand and debug
- Single point of control for complex workflows

**Cons:**
- Orchestrator can become a bottleneck
- Tight coupling to the orchestrator
- Single point of failure

**Use when:**
- Workflow is complex with many steps
- You need centralized control
- Business logic is better centralized

### Choreography
**Pros:**
- Loose coupling between services
- No single point of failure
- Scales better

**Cons:**
- Harder to understand the full flow
- Debugging distributed state is complex
- Can lead to event chain complexity

**Use when:**
- Simple workflows with few steps
- Services are independent
- You want maximum decoupling

## Saga State Machine (Orchestration Example)

```csharp
public enum TripBookingState
{
    Started,
    HotelReserved,
    TicketReserved,
    PaymentProcessed,
    Completed,
    Failed,
    Compensating
}

public class TripBookingSaga
{
    public Guid Id { get; set; }
    public TripBookingState State { get; set; }
    
    public async Task ExecuteAsync()
    {
        try
        {
            State = TripBookingState.Started;
            
            await ReserveHotelAsync();
            State = TripBookingState.HotelReserved;
            
            await ReserveTicketAsync();
            State = TripBookingState.TicketReserved;
            
            await ProcessPaymentAsync();
            State = TripBookingState.PaymentProcessed;
            
            State = TripBookingState.Completed;
        }
        catch (Exception)
        {
            State = TripBookingState.Compensating;
            await CompensateAsync();
            State = TripBookingState.Failed;
            throw;
        }
    }
    
    private async Task CompensateAsync()
    {
        if (State >= TripBookingState.PaymentProcessed)
            await RefundPaymentAsync();
            
        if (State >= TripBookingState.TicketReserved)
            await CancelTicketAsync();
            
        if (State >= TripBookingState.HotelReserved)
            await CancelHotelAsync();
    }
}
```

## Compensating Transactions

Each service must implement compensating operations:

| Service | Forward Action | Compensating Action |
|---------|----------------|---------------------|
| Hotel | Reserve room | Cancel reservation |
| Ticket | Book ticket | Cancel ticket |
| Payment | Charge card | Refund payment |
| Inventory | Reserve items | Release reservation |

## Saga Persistence

Saga state must be persisted to handle failures:

```csharp
public class SagaState
{
    public Guid SagaId { get; set; }
    public string CurrentStep { get; set; }
    public string Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> CompletedSteps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
```

## Running the Demo

### Orchestration (Trip Planner)

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Book a trip (success scenario)
curl -X POST http://localhost:5020/api/saga/v1/trips \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "guid",
    "hotelId": "guid",
    "ticketId": "guid",
    "paymentMethod": "credit_card"
  }'

# Check trip status
curl http://localhost:5020/api/saga/v1/trips/{id}
```

### Choreography (Online Store)

```bash
# Create an order (success scenario)
curl -X POST http://localhost:5030/api/saga/v1/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "guid",
    "items": [
      {"productId": "guid", "quantity": 2}
    ]
  }'

# Check order status
curl http://localhost:5030/api/saga/v1/orders/{id}
```

## Handling Failures

### Timeout Handling
Set timeouts for each step to prevent indefinite waiting:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await ReserveHotelAsync(cts.Token);
```

### Retry with Exponential Backoff
Retry transient failures with exponential backoff:

```csharp
await Polly.Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
    .ExecuteAsync(() => CallServiceAsync());
```

### Idempotency
Ensure operations are idempotent to safely retry:

```csharp
// Use idempotency keys
var idempotencyKey = Guid.NewGuid().ToString();
await ProcessPaymentAsync(amount, idempotencyKey);
```

## Common Pitfalls

1. **Forgetting compensation**: Always implement compensating transactions
2. **Non-idempotent operations**: Make sure operations can be safely retried
3. **Lost messages**: Use reliable messaging with acknowledgments
4. **Saga timeout**: Implement timeouts to prevent sagas from running forever
5. **Concurrent sagas**: Handle race conditions when multiple sagas modify the same data

## Learn More

Watch the detailed video explanation: [Saga Pattern Deep Dive](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez)
