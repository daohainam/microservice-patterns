# MCP Servers for Microservice Patterns

This directory contains Model Context Protocol (MCP) servers that provide programmatic access to the microservice pattern demonstrations in this repository.

## Overview

Each MCP server exposes tools that allow LLMs and other clients to interact with specific microservice patterns through a standardized interface. These servers act as bridges between MCP clients and the underlying microservices.

## Available MCP Servers

### 1. CQRS Library MCP Server

**Location:** `Mcp.CQRS.Library.McpServer`

**Pattern:** Command Query Responsibility Segregation (CQRS)

**Description:** Provides tools to interact with a library management system demonstrating the CQRS pattern with separate read and write models.

**Tools:**
- `GetBooks` - Retrieve all books
- `GetBookDetails` - Get details of a specific book by ID
- `CreateBook` - Add a new book to the library
- `UpdateBook` - Update book information
- `GetBorrowers` - Retrieve all borrowers
- `GetBorrowerDetails` - Get details of a specific borrower by ID
- `CreateBorrower` - Register a new borrower
- `UpdateBorrower` - Update borrower information
- `GetBorrowings` - Retrieve all borrowing transactions
- `GetBorrowingDetails` - Get details of a specific borrowing by ID
- `BorrowBook` - Create a borrowing transaction (check out a book)
- `ReturnBook` - Mark a book as returned
- `GetBorrowingHistoryItems` - Query borrowing history with filters

**Target Services:**
- CQRS.Library.BookService
- CQRS.Library.BorrowerService
- CQRS.Library.BorrowingService
- CQRS.Library.BorrowingHistoryService

---

### 2. Event Sourcing Banking MCP Server

**Location:** `Mcp.EventSourcing.Banking.McpServer`

**Pattern:** Event Sourcing

**Description:** Provides tools to interact with a banking system that uses event sourcing to maintain a complete audit trail of all account changes.

**Tools:**
- `OpenAccount` - Create a new bank account with initial balance and credit limit
- `GetAccount` - Retrieve current account state (rebuilt from event stream)
- `Deposit` - Deposit money into an account
- `Withdraw` - Withdraw money from an account

**Target Service:**
- EventSourcing.Banking.AccountService

**Key Features:**
- All state changes are stored as events
- Current state is reconstructed by replaying events
- Full audit trail of all transactions
- Support for credit limits and overdraft protection

---

### 3. Saga Trip Planner MCP Server (Orchestration)

**Location:** `Mcp.Saga.TripPlanner.McpServer`

**Pattern:** Saga (Orchestration-based)

**Description:** Provides tools to interact with a trip planning system that demonstrates orchestration-based saga pattern for coordinating distributed transactions across hotel, ticket, and payment services.

**Tools:**
- `GetTrips` - Retrieve all trip bookings
- `GetTripDetails` - Get details of a specific trip by ID
- `CreateTrip` - Create a new trip booking (orchestrates hotel reservation, ticket booking, and payment)

**Target Service:**
- Saga.TripPlanner.TripPlanningService (Orchestrator)

**Coordinated Services:**
- Saga.TripPlanner.HotelService
- Saga.TripPlanner.TicketService
- Saga.TripPlanner.PaymentService

**Key Features:**
- Central orchestrator coordinates all saga steps
- Automatic compensation on failure (cancels hotel, tickets if payment fails)
- Clear workflow with retry logic
- Status tracking through saga states

---

### 4. Saga Online Store MCP Server (Choreography)

**Location:** `Mcp.Saga.OnlineStore.McpServer`

**Pattern:** Saga (Choreography-based)

**Description:** Provides tools to interact with an online store that demonstrates choreography-based saga pattern where services react to events without a central coordinator.

**Tools:**
- `GetOrders` - Retrieve all orders
- `GetOrderDetails` - Get details of a specific order by ID
- `CreateOrder` - Place a new order (triggers event chain across inventory and payment services)

**Target Service:**
- Saga.OnlineStore.OrderService

**Coordinated Services (via events):**
- Saga.OnlineStore.InventoryService
- Saga.OnlineStore.PaymentService
- Saga.OnlineStore.CatalogService

**Key Features:**
- No central orchestrator
- Services react to domain events
- Loose coupling between services
- Compensating events for rollback

---

## Running MCP Servers

Each MCP server is a standalone ASP.NET Core application that can be run independently:

```bash
# Run CQRS Library MCP Server
dotnet run --project mcp/Mcp.CQRS.Library.McpServer

# Run Event Sourcing Banking MCP Server
dotnet run --project mcp/Mcp.EventSourcing.Banking.McpServer

# Run Saga Trip Planner MCP Server
dotnet run --project mcp/Mcp.Saga.TripPlanner.McpServer

# Run Saga Online Store MCP Server
dotnet run --project mcp/Mcp.Saga.OnlineStore.McpServer
```

**Note:** MCP servers require the corresponding pattern services to be running. Use the .NET Aspire AppHost to start all services:

```bash
dotnet run --project MicroservicePatterns.AppHost
```

## Architecture

Each MCP server follows a consistent architecture:

```
MCP Server
├── Program.cs              # Server configuration and startup
├── *Tool.cs               # MCP tool definitions with [McpServerTool] attributes
├── I*Service.cs           # Service interface
├── *Service.cs            # HTTP client service implementation
├── Model classes          # DTOs matching the underlying service models
└── appsettings.json       # Configuration
```

### Key Components:

1. **Tools**: Methods decorated with `[McpServerTool]` that are exposed via MCP
2. **Service Layer**: Encapsulates HTTP communication with underlying microservices
3. **Models**: Plain C# classes representing domain entities
4. **HTTP Clients**: Configured to use Aspire service discovery

## Configuration

MCP servers use Aspire service discovery to locate backend services. Service endpoints are configured using the Aspire naming convention:

```csharp
builder.Services.AddHttpClient("servicename",
    static client => client.BaseAddress = new("https+http://Pattern-ServiceName"));
```

## Error Handling

All MCP servers include:
- Input validation with descriptive error messages
- GUID format validation for IDs
- HTTP error propagation from backend services
- Meaningful exceptions for failed operations

## Security Considerations

- MCP servers do not implement authentication - they rely on network-level security
- Input validation is performed on all user-provided parameters
- HTTP clients follow redirects and use standard timeout settings
- Sensitive data (like card details) is passed through to backend services without logging

## Integration with .NET Aspire

MCP servers integrate with .NET Aspire's:
- Service discovery
- Health checks
- Telemetry and logging
- Configuration management

## Learn More

- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Pattern Documentation](../patterns/README.md)
- [Main Repository README](../README.md)
- [Video Tutorials](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez)
