# Identity Service – OAuth2 / OpenID Connect with OpenIddict

This pattern demonstrates how to build a centralised **identity and authorisation server** for a
microservice system using:

| Technology | Role |
|---|---|
| [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity) | User management (registration, login, password hashing, roles) |
| [OpenIddict](https://documentation.openiddict.com/) | OAuth 2.0 / OpenID Connect protocol server |
| [Entity Framework Core + PostgreSQL](https://learn.microsoft.com/ef/core/) | Persistent storage for users, tokens, and OAuth2 clients |

---

## Why a centralised identity service?

In a microservice system, each service should not manage its own user store.
Instead, every service trusts a single **OAuth2 authorisation server** that issues
short-lived tokens.  
Other services then **verify** those tokens without ever talking to the identity service again.

```
┌─────────────────────────────────────────────────────────────────────┐
│                          Client Application                          │
│  (SPA / mobile app / background worker / Postman)                   │
└────────────────────────────┬────────────────────────────────────────┘
                             │  1. Request token
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        Identity Service                              │
│                                                                      │
│  POST /connect/token        ← issues access tokens                  │
│  GET  /connect/authorize    ← starts Authorization Code flow        │
│  POST /connect/logout       ← ends user session                     │
│  GET  /connect/userinfo     ← returns user profile claims           │
│  POST /api/identity/v1/users/register  ← creates a new account      │
└────────────────────────────┬────────────────────────────────────────┘
                             │  2. Access token (JWT)
                             ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Book Service │  │ Order Service│  │  Any Service │
│              │  │              │  │              │
│  Validates   │  │  Validates   │  │  Validates   │
│  JWT locally │  │  JWT locally │  │  JWT locally │
└──────────────┘  └──────────────┘  └──────────────┘
```

---

## Project structure

```
Identity.IdentityService/
├── Program.cs                          ← entry point; wires up middleware
├── GlobalUsings.cs                     ← shared using directives
│
├── Bootstraping/
│   └── ApplicationServiceExtensions.cs ← registers Identity + OpenIddict + DB
│
├── Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs     ← EF Core context (Identity + OpenIddict tables)
│   └── Entity/
│       └── ApplicationUser.cs          ← custom Identity user (add profile fields here)
│
├── Workers/
│   └── OpenIddictSeedWorker.cs         ← seeds OAuth2 clients and demo users on startup
│
├── Apis/
│   ├── ConnectApi.cs                   ← OAuth2 protocol endpoints (token, authorize, …)
│   └── UserApi.cs                      ← user management REST API (register, login, logout)
│
└── Migrations/                         ← EF Core migrations (auto-applied on startup)
```

---

## Supported OAuth2 flows

### 1. Client Credentials (service-to-service)

Used when **no user is involved** – e.g. a background worker calling another microservice.

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=service-client
&client_secret=service-client-secret
&scope=api
```

### 2. Authorization Code + PKCE (user-facing applications)

Used by **SPAs, mobile apps, and interactive clients**.
PKCE (Proof Key for Code Exchange) replaces the client secret for public clients.

**Step 1** – Redirect the user's browser to the authorisation endpoint:

```
GET /connect/authorize
  ?client_id=web-client
  &response_type=code
  &redirect_uri=https://oauth.pstmn.io/v1/callback
  &scope=openid profile email api
  &code_challenge=<PKCE-challenge>
  &code_challenge_method=S256
  &state=<random-state>
```

**Step 2** – Exchange the code for tokens:

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&client_id=web-client
&code=<authorization-code>
&redirect_uri=https://oauth.pstmn.io/v1/callback
&code_verifier=<PKCE-verifier>
```

### 3. Refresh Token

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=web-client
&refresh_token=<refresh-token>
```

---

## User management

### Register a new user

```http
POST /api/identity/v1/users/register
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "Alice123!",
  "displayName": "Alice"
}
```

### Login (obtain Identity cookie for the authorize flow)

```http
POST /api/identity/v1/users/login
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "Alice123!"
}
```

---

## Pre-seeded accounts and OAuth2 clients

The `OpenIddictSeedWorker` creates the following on first startup:

| Type | ID / Email | Secret / Password | Description |
|---|---|---|---|
| OAuth2 client | `service-client` | `service-client-secret` | Client Credentials client |
| OAuth2 client | `web-client` | *(none – public client)* | Authorization Code + PKCE |
| API scope | `api` | – | Scope for protected resources |
| Admin user | `admin@example.com` | `Admin123!` | Admin + User roles |
| Regular user | `user@example.com` | `User123!` | User role only |

> ⚠️ These are **development-only** defaults. Change or remove them before going to production.

---

## How other services can validate tokens

Add `OpenIddict.Validation.AspNetCore` to the consuming service and configure it to
point at this service's discovery document:

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Discover signing keys from the identity service's .well-known endpoint.
        options.SetIssuer("https+http://identity-service");
        options.AddAudiences("resource-server");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
```

Then protect an endpoint with `[Authorize]` or `.RequireAuthorization()`.

---

## Key concepts illustrated

| Concept | Where |
|---|---|
| OpenIddict + ASP.NET Core Identity integration | `Bootstraping/ApplicationServiceExtensions.cs` |
| Client Credentials grant | `Apis/ConnectApi.cs` → `HandleTokenAsync` |
| Authorization Code + PKCE grant | `Apis/ConnectApi.cs` → `HandleAuthorizationAsync` + `HandleTokenAsync` |
| Token claims and destinations | `Apis/ConnectApi.cs` → `GetDestinations` |
| Seeding OAuth2 clients and users at startup | `Workers/OpenIddictSeedWorker.cs` |
| Unified Identity + OpenIddict DbContext | `Infrastructure/Data/ApplicationDbContext.cs` |
