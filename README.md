# MicroservicePatterns – Advanced microservice patterns on .NET Aspire

This repository is a set of **focused, production-inspired demos** showing how to implement complex microservice patterns on **.NET** using **.NET Aspire**.

The goal is simple:

> Strip away domain noise and framework boilerplate so you can clearly see  
> **what each pattern does, why you need it, and how to implement it.**

If you care about **high-quality microservice architecture** on .NET, this repo is for you.

---

## Why this repository?

Most examples of microservices either:

- oversimplify patterns to the point where they are unrealistic, or  
- drown you in domain complexity, infrastructure, and unrelated concerns.

This project takes a different approach:

- **Pattern-first**: Each demo is built around a single architectural problem.
- **Minimal domain**: Just enough domain to make the scenario realistic.
- **Aspire-native**: Uses .NET Aspire to bootstrap and orchestrate services.
- **Teachable**: Designed to be read, explored, and modified.

Use it as a **learning lab**, a **reference implementation**, or a **starting point** for your own experiments.

---

## Patterns covered

### CQRS (Command Query Responsibility Segregation)

**Problem:**  
How do you query data efficiently when it lives in **multiple distributed stores**, and reads start to outnumber writes?

**What this demo shows:**

- Splitting **write models** (commands) from **read models** (queries).
- Building **read models** that aggregate data from multiple services.
- Improving **read performance and scalability** without over-complicating the write side.

---

### Event Sourcing

**Problem:**  
How do you model **complex aggregates** and still keep updates atomic, auditable, and easy to reason about?

**What this demo shows:**

- Representing changes as **events** instead of overwriting state.
- Rebuilding current state by **replaying the event stream**.
- Keeping a **full history** of an entity’s lifecycle for debugging, audits, and analytics.
- Treating complex updates as sequences of **small atomic events**.

---

### Saga (Orchestration & Choreography)

**Problem:**  
How do you implement a **business transaction spanning multiple microservices** without using distributed transactions?

**What these demos show:**

- **Orchestration-based Sagas**  
  - A central **Saga orchestrator** coordinates each step.
  - Clear flow and central control.

- **Choreography-based Sagas**  
  - Services react to **domain events** and perform their own steps.
  - No central brain; the flow emerges from **event handling**.

You’ll see how to handle **success, failure, retries, and compensation** across services.

---

### Transactional Outbox

**Problem:**  
How do you ensure that when a service updates its database **and** publishes an integration event, **either both happen or neither do**?

**What this demo shows:**

- Writing changes and outgoing events into the **same database transaction**.
- Using an **outbox table** and a background process to publish events reliably.
- Avoiding the classic “DB updated, message not sent” inconsistency.

This pattern underpins reliable **event-driven communication** between services.

---

### Webhook

**Problem:**  
How do you build **event-driven integrations** where external systems subscribe to your events instead of you polling them?

**What this demo shows:**

- Registering and managing **webhook subscriptions**.
- Sending **notifications** to third-party endpoints when events occur.
- Basic patterns for **retries**, **verification**, and **security** around webhooks.

This is the backbone for “notify me when X happens” style integrations.

---

## Supporting building blocks

### CloudEvents

Standardizing event envelopes using the **CloudEvents** specification makes it much easier to:

- Integrate with other platforms and languages.
- Route and inspect events in a consistent way.
- Treat your system as a set of **composable event-driven services**.

The demos show how to emit and consume events using the CloudEvents format.

---

### Mediator

To reduce tight coupling between parts of your system, this repo includes a **Mediator** implementation with the same goals as libraries like MediatR, but with a **fresh implementation** (after MediatR’s license change).

You’ll see how to:

- Send **commands** and **queries** without components depending directly on each other.
- Centralize cross-cutting behaviors (logging, validation, etc.) via **pipeline behaviors**.

---

### Circuit Breaker

Distributed systems fail in **messy** ways.

The Circuit Breaker demos show how to:

- Fail fast when a downstream service is **unhealthy**.
- Prevent cascading failures from taking down your system.
- Gradually **recover** when the remote service stabilizes.

This is one of the most important patterns for **resilient microservices**.

---

## Repository layout

At a high level:

- `MicroservicePatterns.AppHost`  
  The **.NET Aspire AppHost** project that wires everything together and orchestrates the services.

- `patterns/`  
  The individual pattern demos (CQRS, Event Sourcing, Saga, etc.).  
  Each pattern is organized to keep the focus on **the pattern itself**, not on an over-engineered domain.

- `EventBus`, `EventBus.Kafka`  
  Infrastructure for publishing/consuming integration events.  
  Includes a Kafka-based implementation to show how patterns play with a real event bus.

- `Mediator`  
  The custom Mediator implementation used by the demos.

- `MicroservicePatterns.Shared`, `MicroservicePatterns.ServiceDefaults`, `MicroservicePatterns.DatabaseMigrationHelpers`  
  Shared abstractions, defaults, and helpers to keep the pattern demos **clean and readable**.

You can browse the `patterns/` folder and open the pattern you’re interested in; each is built to be read in isolation.

---

## Getting started

### Prerequisites

- **.NET SDK** (latest version that supports .NET Aspire)  
- **.NET Aspire** workloads and toolings installed  
  (follow the official .NET Aspire documentation for your platform).
- Optionally: **Docker** and/or **Kafka**, if you want to run the Kafka-based demos locally.

### Run the solution

1. Clone the repo:

   ```bash
   git clone https://github.com/daohainam/microservice-patterns.git
   cd microservice-patterns