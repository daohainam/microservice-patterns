# CQRS Pattern Implementation

## Overview

This demo shows how to implement the **Command Query Responsibility Segregation (CQRS)** pattern in a microservices architecture using .NET Aspire.

## Problem Statement

When data lives in multiple distributed stores and reads start to outnumber writes, how do you query data efficiently while maintaining good write performance?

## Solution

CQRS separates the write path (commands) from the read path (queries):

- **Write models** handle commands and maintain transactional consistency
- **Read models** are optimized for queries and can aggregate data from multiple sources
- Changes in write models are propagated to read models via integration events

## Services

### BookService
Manages the book catalog (write model).

**Endpoints:**
- `POST /api/cqrs/v1/books` - Add a new book
- `GET /api/cqrs/v1/books/{id}` - Get book details

### BorrowerService
Manages borrower information (write model).

**Endpoints:**
- `POST /api/cqrs/v1/borrowers` - Register a borrower
- `GET /api/cqrs/v1/borrowers/{id}` - Get borrower details

### BorrowingService
Manages borrowing transactions (write model).

**Endpoints:**
- `POST /api/cqrs/v1/borrowings` - Create a borrowing
- `GET /api/cqrs/v1/borrowings` - List borrowings

### BorrowingHistoryService
Provides a denormalized read model combining book, borrower, and borrowing data.

**Endpoints:**
- `GET /api/cqrs/v1/history/items` - Query borrowing history with filters

## Flow

1. Book is added via BookService → `BookCreatedIntegrationEvent` published
2. Borrower is registered via BorrowerService → `BorrowerCreatedIntegrationEvent` published
3. Borrowing is created via BorrowingService → `BorrowingCreatedIntegrationEvent` published
4. BorrowingHistoryService consumes all events and updates its denormalized read model
5. Queries against BorrowingHistoryService return fast, pre-aggregated data

## Key Benefits

- **Scalability**: Read and write sides can be scaled independently
- **Performance**: Read models are optimized for specific query patterns
- **Flexibility**: Multiple read models can be created from the same write models
- **Simplicity**: Each model is focused on its specific responsibility

## Running the Demo

```bash
# Start the Aspire app host
dotnet run --project ../../MicroservicePatterns.AppHost

# Add a book
curl -X POST http://localhost:5000/api/cqrs/v1/books \
  -H "Content-Type: application/json" \
  -d '{"id":"guid","title":"Clean Code","author":"Robert C. Martin"}'

# Query the history (wait a few seconds for event propagation)
curl http://localhost:5003/api/cqrs/v1/history/items?bookId=guid
```

## Learn More

Watch the detailed video explanation: [CQRS Pattern Deep Dive](https://www.youtube.com/playlist?list=PLRLJQuuRRcFlOIjMY9w5aoCf6e368oxez)
