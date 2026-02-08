# WebHook Pattern Implementation

## Overview

This demo shows how to implement the **WebHook** pattern for event-driven integrations. WebHooks enable third-party systems to subscribe to events in your application and receive real-time notifications when those events occur.

## Problem Statement

How do you build event-driven integrations where external systems need to be notified when something happens in your application, without your application needing to know about all possible consumers? How do you avoid polling and enable real-time notifications?

## Solution

The WebHook pattern allows external systems to:
1. **Register** a callback URL with your service
2. **Receive HTTP POST notifications** when events occur
3. **Unregister** when they no longer want notifications

Your service:
1. Stores subscriber URLs in a database
2. Listens for domain events
3. Dispatches HTTP POST requests to all registered WebHooks
4. Handles retries and failures

## Architecture

This implementation consists of three main components:

### 1. WebHook Delivery Service
The main API service that manages WebHook subscriptions.

**Endpoints:**
- `POST /api/webhook/v1/webhooks` - Register a WebHook subscription
- `PUT /api/webhook/v1/webhooks/{id}/unregister` - Unregister a WebHook
- `POST /api/deliveryservice/v1/deliveries` - Trigger a delivery (for testing)

### 2. Event Consumer Service
A background service that listens for domain events (e.g., delivery created, delivery status changed) and enqueues WebHook delivery jobs.

### 3. Dispatch Service
A background worker that:
- Reads queued WebHook deliveries from the database
- Makes HTTP POST requests to subscriber URLs
- Handles retries with exponential backoff
- Updates delivery status

## How It Works

### Registration Flow

```
1. Client → POST /api/webhook/v1/webhooks
           {
             "url": "https://client.com/webhooks/deliveries"
           }
           
2. Service generates secret key
           ↓
3. Service saves subscription to database
           ↓
4. Service returns subscription details
           {
             "id": "guid",
             "url": "https://client.com/webhooks/deliveries",
             "secretKey": "secret123"
           }
```

### Event Notification Flow

```
1. Domain Event occurs (e.g., DeliveryCreated)
           ↓
2. EventConsumer receives event
           ↓
3. EventConsumer queries all WebHook subscriptions
           ↓
4. EventConsumer creates delivery queue items
           ↓
5. DispatchService picks up queue items
           ↓
6. DispatchService sends HTTP POST to subscriber URL
   Headers:
   - Content-Type: application/json
   - X-Webhook-Signature: HMAC of payload
   Body: Event data as JSON
           ↓
7. Subscriber returns 200-299 → Success
   OR
   Subscriber returns error → Retry with exponential backoff
```

## Database Schema

### WebHookSubscriptions Table
```sql
CREATE TABLE WebHookSubscriptions (
    Id UUID PRIMARY KEY,
    Url VARCHAR(2000) NOT NULL,
    SecretKey VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL
);
```

### DeliveryQueue Table
```sql
CREATE TABLE DeliveryQueue (
    Id UUID PRIMARY KEY,
    SubscriptionId UUID NOT NULL,
    Url VARCHAR(2000) NOT NULL,
    Payload TEXT NOT NULL,
    Status VARCHAR(20) NOT NULL, -- Pending, Sent, Failed
    RetryCount INT NOT NULL DEFAULT 0,
    NextRetryAt TIMESTAMP NULL,
    CreatedAt TIMESTAMP NOT NULL,
    SentAt TIMESTAMP NULL
);
```

## Security

### Secret Key
When a WebHook is registered, a secret key is generated and returned to the subscriber. This key is used to:

1. **Verify subscription ownership** during unregistration
2. **Sign WebHook payloads** using HMAC-SHA256
3. **Validate that requests came from your service**

### Signature Verification (Subscriber Side)

```csharp
// Your service sends
POST https://subscriber.com/webhook
X-Webhook-Signature: sha256=abc123def456...
Content-Type: application/json

{"eventType":"DeliveryCreated","deliveryId":"..."}

// Subscriber verifies
var payload = await ReadBodyAsString(request);
var signature = request.Headers["X-Webhook-Signature"];
var expectedSignature = ComputeHMAC(payload, secretKey);

if (signature != expectedSignature)
{
    return Unauthorized();
}
```

## Retry Strategy

The dispatch service implements exponential backoff for failed deliveries:

```
Attempt 1: Immediate
Attempt 2: Wait 1 minute
Attempt 3: Wait 2 minutes
Attempt 4: Wait 4 minutes
Attempt 5: Wait 8 minutes
...
Max: 10 attempts, then mark as permanently failed
```

**Successful response codes:** 200-299
**Retry codes:** 408, 429, 500-599
**Permanent failure codes:** 400-407, 410-428, 430-499

## Key Benefits

- **Real-time notifications**: No polling required
- **Decoupling**: Publishers don't need to know about subscribers
- **Scalability**: Subscribers can be added/removed dynamically
- **Flexibility**: Any HTTP-capable system can subscribe
- **Reliability**: Built-in retry mechanism

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Register a WebHook (you'll need a publicly accessible URL)
curl -X POST http://localhost:5000/api/webhook/v1/webhooks \
  -H "Content-Type: application/json" \
  -d '{"url":"https://your-domain.com/webhook-receiver"}'

# Response:
{
  "id": "12345678-1234-1234-1234-123456789012",
  "url": "https://your-domain.com/webhook-receiver",
  "secretKey": "secret_abc123"
}

# Trigger an event (for testing)
curl -X POST http://localhost:5000/api/deliveryservice/v1/deliveries \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer123",
    "pickupAddress": "123 Main St",
    "deliveryAddress": "456 Oak Ave"
  }'

# Your webhook receiver should receive a POST request with the event data

# Unregister the WebHook
curl -X PUT http://localhost:5000/api/webhook/v1/webhooks/12345678-1234-1234-1234-123456789012/unregister \
  -H "Content-Type: application/json" \
  -d '{
    "url":"https://your-domain.com/webhook-receiver",
    "secretKey":"secret_abc123"
  }'
```

## When to Use

✅ **Use WebHooks when:**
- Building integrations with third-party services
- Notifying external systems of events in real-time
- Implementing event-driven architectures across organizational boundaries
- Enabling customers to build custom integrations with your platform
- Replacing polling-based integrations

❌ **Avoid WebHooks when:**
- Communicating between your own microservices (use message queue instead)
- Events are high-frequency (thousands per second)
- Subscribers need guaranteed ordering
- You need synchronous responses from subscribers

## Trade-offs

### Pros
- ✅ Real-time event delivery
- ✅ No polling overhead
- ✅ Subscribers can be added/removed dynamically
- ✅ Works with any HTTP-capable system
- ✅ Publisher remains decoupled from subscribers

### Cons
- ❌ Subscribers must be publicly accessible (or use tunneling like ngrok)
- ❌ No guaranteed delivery (though retries help)
- ❌ No guaranteed ordering
- ❌ Requires handling subscriber failures
- ❌ Security considerations (authentication, signatures)

## Common Pitfalls

1. **Not securing WebHook payloads**
   - Always use HMAC signatures to verify authenticity
   - Consider HTTPS-only URLs

2. **Not handling subscriber failures**
   - Implement retries with exponential backoff
   - Set a maximum retry limit
   - Provide dead letter queue for permanently failed deliveries

3. **Sending too much data**
   - Keep payloads small - send IDs and let subscribers fetch details if needed
   - Consider payload size limits

4. **Not providing subscription verification**
   - Implement a challenge-response mechanism on registration
   - Let subscribers prove they own the URL

5. **Ignoring subscriber timeouts**
   - Set reasonable HTTP client timeouts (e.g., 30 seconds)
   - Don't let slow subscribers block your dispatch service

6. **Not versioning webhook payloads**
   - Include API version in payload or URL
   - Support backward compatibility

## Best Practices

### For Publishers (Your Service)

1. **Idempotency**: Send a unique event ID so subscribers can deduplicate
2. **Timestamp**: Include when the event occurred
3. **Retry Logic**: Implement exponential backoff
4. **Monitoring**: Track delivery success rates
5. **Documentation**: Provide clear payload schemas
6. **Testing**: Offer a webhook testing tool or playground

### For Subscribers (External Systems)

1. **Respond Quickly**: Return 200 OK immediately, process async
2. **Verify Signatures**: Always validate HMAC signatures
3. **Idempotency**: Handle duplicate events gracefully
4. **Error Handling**: Return appropriate HTTP status codes
5. **Testing**: Use tools like ngrok for local development

## Example Webhook Receiver (Subscriber)

```csharp
[HttpPost("webhook-receiver")]
public async Task<IActionResult> ReceiveWebhook(
    [FromHeader(Name = "X-Webhook-Signature")] string signature,
    [FromBody] JsonDocument payload)
{
    // 1. Verify signature
    var payloadString = payload.RootElement.GetRawText();
    var expectedSignature = ComputeHMAC(payloadString, _secretKey);
    if (signature != expectedSignature)
    {
        return Unauthorized("Invalid signature");
    }

    // 2. Extract event data
    var eventType = payload.RootElement.GetProperty("eventType").GetString();
    var eventId = payload.RootElement.GetProperty("eventId").GetString();

    // 3. Check for duplicates (idempotency)
    if (await _processedEvents.ContainsAsync(eventId))
    {
        return Ok(); // Already processed
    }

    // 4. Queue for async processing
    await _eventQueue.EnqueueAsync(new WebhookEvent
    {
        EventId = eventId,
        EventType = eventType,
        Payload = payload
    });

    // 5. Return success immediately
    return Ok();
}
```

## Monitoring and Observability

Key metrics to track:

1. **Delivery Success Rate**: Percentage of successful deliveries
2. **Average Delivery Time**: How long it takes to deliver
3. **Retry Count Distribution**: How many deliveries require retries
4. **Failed Deliveries**: Permanently failed deliveries need attention
5. **Subscriber Health**: Which subscribers are consistently failing

## Learn More

For a detailed explanation of the WebHook pattern, watch the [Microservice Patterns playlist](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez).

## Related Patterns

- **Event Sourcing**: Generate events that can trigger WebHooks
- **Transactional Outbox**: Ensure events aren't lost before WebHook dispatch
- **Idempotent Consumer**: Subscribers should be idempotent
- **Circuit Breaker**: Protect dispatch service from failing subscribers

## Real-World Examples

- **GitHub**: Notify CI/CD systems of code pushes
- **Stripe**: Notify applications of payment events
- **Twilio**: Notify applications of SMS/call events
- **Shopify**: Notify apps of order/product changes
- **Slack**: Notify external systems of channel messages
