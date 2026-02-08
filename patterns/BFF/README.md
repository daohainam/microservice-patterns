# Backend for Frontend (BFF) Pattern Implementation

## Overview

This demo shows how to implement the **Backend for Frontend (BFF)** pattern in a microservices architecture. The BFF pattern creates a dedicated backend service for each frontend application or client type, optimizing the API for that specific client's needs.

## Problem Statement

When building microservices that serve multiple types of clients (web, mobile, POS, partner APIs), how do you:
- Avoid forcing all clients to make multiple API calls to assemble data?
- Prevent mobile apps from downloading unnecessary data?
- Provide each client with an API that matches its specific use case?
- Avoid bloating a single API gateway with client-specific logic?

## Solution

The BFF pattern creates a dedicated backend service for each client type. Each BFF:
- Aggregates data from multiple microservices
- Transforms and shapes data for its specific client
- Handles client-specific business logic
- Optimizes for client-specific performance needs (e.g., mobile bandwidth)

```
┌─────────────┐       ┌─────────────────┐       ┌──────────────┐
│   Web App   │──────▶│   BFF for Web   │──────▶│              │
└─────────────┘       └─────────────────┘       │              │
                                                 │              │
┌─────────────┐       ┌─────────────────┐       │  Catalog     │
│ Mobile App  │──────▶│ BFF for Mobile  │──────▶│  Service     │
└─────────────┘       └─────────────────┘       │              │
                                                 │              │
┌─────────────┐       ┌─────────────────┐       │              │
│  POS System │──────▶│  BFF for POS    │──────▶│              │
└─────────────┘       └─────────────────┘       └──────────────┘
                                                       │
                                                       │
                                              ┌────────▼────────┐
                                              │ Search Service  │
                                              │  (Elasticsearch)│
                                              └─────────────────┘
```

## Services

### ProductCatalogService
The main product catalog service that manages product data.

**Responsibilities:**
- CRUD operations for products, brands, categories, groups
- Publishing product events
- Managing product inventory

### SearchService
A specialized search service using Elasticsearch for fast product search.

**Responsibilities:**
- Full-text search across products
- Filtering by brand, category, price
- Fast, optimized read operations

### SearchSyncService
Background service that keeps Elasticsearch in sync with the catalog database.

**Responsibilities:**
- Consumes product events from the event bus
- Updates search index when products change
- Maintains eventual consistency

### BackendForPOS (BFF)
A specialized backend service for Point-of-Sale systems.

**Endpoints:**
- `GET /api/bff/v1/products?query={text}&catId={guid}&brandId={guid}` - Search products optimized for POS

**Optimizations:**
- Returns only fields needed by POS (name, price, barcode, stock)
- Aggregates data from catalog and search
- Filters out products not available for retail
- Includes real-time stock information

## Key Concepts

### 1. Client-Specific Optimization

Each BFF returns data tailored to its client:

**Web BFF** might return:
```json
{
  "id": "123",
  "name": "Laptop",
  "description": "Full product description...",
  "price": 1299.99,
  "images": [
    { "url": "large.jpg", "size": "1920x1080" },
    { "url": "medium.jpg", "size": "800x600" },
    { "url": "thumb.jpg", "size": "200x200" }
  ],
  "reviews": [...],
  "relatedProducts": [...]
}
```

**Mobile BFF** might return:
```json
{
  "id": "123",
  "name": "Laptop",
  "shortDesc": "High performance laptop",
  "price": 1299.99,
  "image": "thumb.jpg"
}
```

**POS BFF** might return:
```json
{
  "id": "123",
  "name": "Laptop",
  "sku": "LAP-001",
  "price": 1299.99,
  "stock": 15
}
```

### 2. Backend Aggregation

Instead of the client making multiple API calls:

```
// Without BFF (client makes 3 calls)
GET /products/123          → Product details
GET /reviews?productId=123 → Reviews
GET /inventory/123         → Stock info

// With BFF (client makes 1 call)
GET /bff/products/123      → All data aggregated
```

### 3. Search Integration

The POS BFF integrates with Elasticsearch for fast product search:

```csharp
// Search across product name, description, brand, category
var searchQuery = new BoolQuery
{
    Must = [new MultiMatchQuery 
    { 
        Fields = ["name", "description", "brandName", "categoryName"],
        Query = searchText 
    }],
    Filter = [
        new TermQuery { Field = "categoryId", Value = categoryId },
        new TermQuery { Field = "brandId", Value = brandId }
    ]
};
```

## Flow Example: Product Search in POS

```
1. Cashier searches "laptop" in POS
           ↓
2. POS sends: GET /api/bff/v1/products?query=laptop
           ↓
3. BFF for POS receives request
           ↓
4. BFF queries Elasticsearch with filters for:
   - Full-text search on "laptop"
   - Only retail products
   - Available stock > 0
           ↓
5. Elasticsearch returns matching product IDs
           ↓
6. BFF fetches additional data if needed (e.g., real-time stock)
           ↓
7. BFF transforms and filters data for POS needs
           ↓
8. BFF returns: [{sku, name, price, stock}, ...]
           ↓
9. POS displays results to cashier
```

## Key Benefits

- **Optimized APIs**: Each client gets exactly what it needs
- **Performance**: Reduced network calls, optimized payload sizes
- **Autonomy**: Frontend teams can evolve their BFF independently
- **Simplicity**: Frontend code is simpler (one API call instead of many)
- **Flexibility**: Different clients can have different authentication, rate limits, etc.

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# The following services will start:
# - ProductCatalogService (port 5000)
# - SearchService (Elasticsearch)
# - SearchSyncService (background worker)
# - BackendForPOS (port 5001)

# Add a product via the catalog service
curl -X POST http://localhost:5000/api/catalog/v1/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "brandId": "brand-guid",
    "categoryId": "category-guid"
  }'

# Wait a few seconds for search sync...

# Search via POS BFF
curl "http://localhost:5001/api/bff/v1/products?query=laptop"

# Response (optimized for POS):
[
  {
    "id": "product-guid",
    "name": "Laptop",
    "sku": "LAP-001",
    "price": 1299.99,
    "stockQuantity": 15
  }
]
```

## When to Use

✅ **Use BFF when:**
- Serving multiple client types (web, mobile, desktop, partners)
- Clients need different data shapes or aggregations
- Mobile apps need smaller payloads
- Different clients have different security/auth requirements
- Frontend teams want to evolve independently

❌ **Avoid BFF when:**
- You have only one client type
- All clients need the same data
- Your microservices already provide perfect APIs for clients
- The overhead of maintaining multiple BFFs outweighs the benefits

## Trade-offs

### Pros
- ✅ Optimized for each client type
- ✅ Reduced network calls from client
- ✅ Frontend team autonomy
- ✅ Can evolve independently
- ✅ Better mobile performance (smaller payloads)
- ✅ Can implement client-specific caching

### Cons
- ❌ More services to maintain
- ❌ Potential code duplication across BFFs
- ❌ Need to coordinate when backend services change
- ❌ More complex deployment
- ❌ Risk of BFFs becoming "mini-monoliths"

## Common Pitfalls

1. **Creating one BFF per screen**
   - BFF should be per client type (web, mobile), not per screen
   - Avoid over-fragmenting your architecture

2. **Duplicating business logic in BFFs**
   - BFFs should only have presentation logic
   - Business rules belong in domain microservices

3. **Making BFF a dumb proxy**
   - BFF should aggregate and transform, not just forward requests
   - If it's just proxying, you don't need a BFF

4. **Not sharing common code**
   - Use shared libraries for common functionality
   - Don't duplicate DTOs, validation, etc.

5. **Forgetting about authentication**
   - BFFs should authenticate clients
   - Use appropriate auth for each client type (OAuth for web, API keys for partners, etc.)

6. **Creating too many BFFs**
   - Start with broad categories (web, mobile, partner)
   - Don't create a BFF for every minor difference

## Best Practices

### 1. Keep BFFs Thin
```csharp
// ❌ Bad: Business logic in BFF
if (product.Stock < 10)
{
    product.Status = "LowStock";
    await SendLowStockAlert(product);
}

// ✅ Good: BFF only transforms data
return new POSProduct
{
    Id = product.Id,
    Name = product.Name,
    Price = product.Price,
    StockLevel = product.Stock > 10 ? "Good" : "Low"
};
```

### 2. Use GraphQL for Flexible BFFs
```csharp
// Consider GraphQL for BFFs when clients need
// different combinations of data
query POSProductSearch {
  products(query: "laptop") {
    id
    name
    price
    stock
  }
}
```

### 3. Implement Caching
```csharp
// Cache aggregated data in BFF
var cacheKey = $"pos-product-{id}";
var product = await _cache.GetOrCreateAsync(cacheKey, async entry =>
{
    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
    return await AggregateProductData(id);
});
```

### 4. Monitor BFF Performance
```csharp
// Track how many backend calls each BFF endpoint makes
_metrics.Increment("bff.backend_calls", tags: ["endpoint:search", "service:catalog"]);
_metrics.Histogram("bff.response_time", responseTime, tags: ["endpoint:search"]);
```

## Search Integration Pattern

This demo showcases integrating Elasticsearch for fast product search:

### 1. Indexing (SearchSyncService)
```csharp
// Listen for product events
await _eventBus.Subscribe<ProductCreatedEvent>(async evt =>
{
    var doc = new ProductIndexDocument
    {
        Id = evt.ProductId,
        Name = evt.Name,
        Description = evt.Description,
        BrandName = evt.BrandName,
        CategoryName = evt.CategoryName,
        Price = evt.Price
    };
    await _elasticClient.IndexAsync(doc);
});
```

### 2. Searching (BFF)
```csharp
// Fast full-text search
var results = await _elasticClient.SearchAsync<ProductIndexDocument>(s => s
    .Query(q => q
        .MultiMatch(m => m
            .Fields(f => f
                .Field(p => p.Name)
                .Field(p => p.Description)
                .Field(p => p.BrandName))
            .Query(searchText)))
    .Size(10));
```

## Learn More

For a detailed explanation of the BFF pattern, watch the [Microservice Patterns playlist](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez).

## Related Patterns

- **API Gateway**: BFF is often placed behind an API Gateway
- **CQRS**: BFFs naturally align with query models
- **Aggregator Pattern**: BFFs aggregate data from multiple services
- **Event-Driven**: SearchSync uses events to keep search index updated

## Alternative Approaches

- **GraphQL**: Clients specify exactly what data they need
- **OData**: Flexible query capabilities for clients
- **API Gateway with transformation**: Single gateway with client-specific transformation rules
- **Micro-frontends**: Each frontend owns its own BFF
