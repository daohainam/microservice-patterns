# Resilience Patterns Implementation

## Overview

This library implements **Resilience Patterns** for building fault-tolerant microservices. The primary focus is on the **Circuit Breaker** pattern, which prevents cascading failures in distributed systems by detecting failures and temporarily blocking calls to unhealthy services.

## Problem Statement

In distributed systems, when a downstream service fails or becomes slow, how do you:
- Prevent your service from wasting resources on doomed requests?
- Avoid cascading failures that bring down your entire system?
- Give the failing service time to recover?
- Quickly detect when the service is healthy again?

## Solution

The Circuit Breaker pattern acts like an electrical circuit breaker - it monitors for failures and "trips" to prevent further calls when a threshold is exceeded.

### Circuit Breaker States

```
┌─────────┐
│ Closed  │ ──[threshold failures]──> ┌──────┐
│(Normal) │                            │ Open │
└─────────┘ <──[success threshold]──   │(Block)│
     ▲                                 └──────┘
     │                                     │
     │                                     │
     │                                     │[timeout expires]
     │                                     ▼
     │                              ┌──────────┐
     └──────[success]───────────────│Half-Open │
                                    │  (Test)  │
                                    └──────────┘
```

1. **Closed**: Normal operation, requests flow through
   - Monitors for failures
   - If failures exceed threshold → Open

2. **Open**: Circuit is "tripped", requests immediately fail
   - Stops wasting resources on failing service
   - After timeout period → Half-Open

3. **Half-Open**: Testing if service recovered
   - Allows a few test requests through
   - If successful → Closed
   - If failed → Open

## Architecture

### Core Interface

```csharp
public interface IResilienceStrategy<T>
{
    Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, 
                        CancellationToken cancellationToken = default);
}
```

### Circuit Breaker Strategy

The `CircuitBreakerResilienceStrategy<T>` wraps any async operation and applies circuit breaker logic.

**Configuration:**
- `FailureThreshold`: Number of failures before opening circuit (default: 5)
- `SuccessThreshold`: Number of successes before closing circuit (default: 2)
- `DurationOfBreak`: How long to stay open before testing (default: 30 seconds)

## Usage Example

```csharp
// Configure circuit breaker
var context = new CircuitBreakerResilienceStrategyContext
{
    FailureThreshold = 5,
    SuccessThreshold = 2,
    DurationOfBreak = TimeSpan.FromSeconds(30)
};

var circuitBreaker = new CircuitBreakerResilienceStrategy<string>(context);

// Wrap your service call
try
{
    var result = await circuitBreaker.ExecuteAsync(async ct =>
    {
        // Call to potentially failing downstream service
        return await httpClient.GetStringAsync("http://downstream-service/api/data", ct);
    }, cancellationToken);
    
    Console.WriteLine($"Success: {result}");
}
catch (CircuitBreakerOpenException)
{
    Console.WriteLine("Circuit breaker is open - service unavailable");
    // Return cached data or fallback response
}
catch (Exception ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
}
```

## How It Works

### Normal Operation (Closed State)

```csharp
// Requests pass through
Request 1: ✓ Success (count: 1 success)
Request 2: ✓ Success (count: 2 success)
Request 3: ✗ Failure (count: 1 failure)
Request 4: ✗ Failure (count: 2 failures)
Request 5: ✗ Failure (count: 3 failures)
Request 6: ✗ Failure (count: 4 failures)
Request 7: ✗ Failure (count: 5 failures) → TRIP CIRCUIT → OPEN
```

### Circuit Open (Blocking Requests)

```csharp
// All requests immediately fail with CircuitBreakerOpenException
Request 8: ✗ CircuitBreakerOpenException (0ms)
Request 9: ✗ CircuitBreakerOpenException (0ms)
// Wait 30 seconds...
Request 10: → HALF-OPEN (test request allowed)
```

### Circuit Half-Open (Testing Recovery)

```csharp
Request 10: ✓ Success (count: 1 success)
Request 11: ✓ Success (count: 2 successes) → CLOSE CIRCUIT
// Circuit is now closed, normal operation resumes
```

## Key Benefits

- **Fail Fast**: Don't wait for timeouts when service is known to be down
- **Resource Protection**: Prevent thread pool exhaustion from hanging calls
- **Automatic Recovery**: Automatically test and recover when service is healthy
- **Cascading Failure Prevention**: Stop failures from propagating through the system
- **System Stability**: Maintain overall system health even when components fail

## When to Use

✅ **Use Circuit Breaker when:**
- Calling external HTTP APIs or microservices
- Accessing databases or remote resources
- Any operation that can fail and cause cascading problems
- Implementing retry logic (combine with circuit breaker)
- Building production systems that need high availability

❌ **Don't use Circuit Breaker when:**
- Local in-process operations
- Operations that should never be blocked (critical system functions)
- Already handled by infrastructure (e.g., service mesh like Istio)

## Trade-offs

### Pros
- ✅ Prevents cascading failures
- ✅ Fails fast when service is down
- ✅ Automatic recovery detection
- ✅ Improves overall system resilience
- ✅ Reduces resource waste

### Cons
- ❌ Adds complexity to error handling
- ❌ May block requests even if only some instances are failing
- ❌ Requires tuning thresholds for your use case
- ❌ Can hide problems if not monitored

## Common Pitfalls

1. **Setting thresholds too low**
   - Don't trip the circuit on the first failure - transient errors are normal
   - Recommendation: Start with 5-10 failures

2. **Setting timeout too short**
   - Give services enough time to recover
   - Recommendation: Start with 30-60 seconds

3. **Not logging circuit state changes**
   - Always log when circuit opens/closes for monitoring

4. **Forgetting fallback behavior**
   - Always have a plan for when the circuit is open (cached data, degraded mode, etc.)

5. **Applying to wrong operations**
   - Don't apply to fast, local operations
   - Don't apply to operations that must never fail

6. **Not monitoring circuit state**
   - Expose circuit state metrics (open/closed/half-open counts)
   - Alert when circuits are open for too long

## Configuration Guidelines

### Choosing Failure Threshold

```csharp
// Low traffic service (few requests per minute)
FailureThreshold = 3

// Medium traffic service (requests per second)
FailureThreshold = 5-10

// High traffic service (many requests per second)
FailureThreshold = 20-50
```

### Choosing Duration of Break

```csharp
// Fast-recovering services (in-memory cache, local services)
DurationOfBreak = TimeSpan.FromSeconds(10)

// Medium recovery time (databases, typical APIs)
DurationOfBreak = TimeSpan.FromSeconds(30-60)

// Slow recovery (external services, batch processes)
DurationOfBreak = TimeSpan.FromMinutes(2-5)
```

### Choosing Success Threshold

```csharp
// Conservative (wait for more proof of recovery)
SuccessThreshold = 5-10

// Balanced (default)
SuccessThreshold = 2-3

// Aggressive (trust recovery quickly)
SuccessThreshold = 1
```

## Integration with Other Patterns

### With Retry Pattern
```csharp
// Combine: Retry within circuit breaker
await circuitBreaker.ExecuteAsync(async ct =>
{
    return await retryPolicy.ExecuteAsync(async () =>
    {
        return await httpClient.GetAsync(url);
    });
});
```

### With Timeout Pattern
```csharp
// Apply timeout before circuit breaker
await circuitBreaker.ExecuteAsync(async ct =>
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    return await httpClient.GetAsync(url, cts.Token);
});
```

### With Fallback Pattern
```csharp
try
{
    return await circuitBreaker.ExecuteAsync(async ct =>
        await GetDataFromService(ct));
}
catch (CircuitBreakerOpenException)
{
    // Fallback to cached data
    return await GetDataFromCache();
}
```

## Monitoring and Observability

Key metrics to track:

1. **Circuit State**: Current state (Closed/Open/Half-Open)
2. **Failure Count**: How close to threshold
3. **Success Count**: In half-open state
4. **Time in Open State**: How long circuit has been open
5. **State Transitions**: When circuit opens/closes

## Learn More

For a detailed explanation of resilience patterns, watch the [Microservice Patterns playlist](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez).

## Related Patterns

- **Retry Pattern**: Automatically retry failed operations (use with circuit breaker)
- **Timeout Pattern**: Set maximum time for operations
- **Bulkhead Pattern**: Isolate resources to prevent total system failure
- **Rate Limiting**: Control request rate to prevent overload

## Alternative Implementations

For production use, consider:
- [Polly](https://github.com/App-vNext/Polly) - Comprehensive .NET resilience library
- Service Mesh (Istio, Linkerd) - Infrastructure-level resilience
- Cloud Platform features (Azure, AWS) - Managed resilience patterns
