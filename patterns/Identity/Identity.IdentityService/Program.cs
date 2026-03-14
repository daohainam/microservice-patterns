// ─────────────────────────────────────────────────────────────────────────────
// Identity Service – Program entry point
//
// This service implements an OAuth2 / OpenID Connect authorization server using:
//   • ASP.NET Core Identity  – user management (registration, login, roles, …)
//   • OpenIddict              – OAuth2 / OIDC protocol handling
//   • PostgreSQL via EF Core  – persistent storage for users, tokens, and clients
//
// Supported OAuth2 flows:
//   • Authorization Code + PKCE  – for user-facing applications (SPA, native apps)
//   • Client Credentials          – for service-to-service communication
//   • Refresh Token               – token renewal without re-authentication
// ─────────────────────────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Authentication and authorization middleware must be added before mapping endpoints.
app.UseAuthentication();
app.UseAuthorization();

// Map OAuth2 / OIDC connect endpoints (token, authorize, logout, userinfo).
app.MapConnectApi();

// Map user management endpoints (registration, etc.).
app.MapUserApi();

// Apply pending EF Core migrations on startup (creates tables if they don't exist).
await app.MigrateDbContextAsync<ApplicationDbContext>();

app.Run();
