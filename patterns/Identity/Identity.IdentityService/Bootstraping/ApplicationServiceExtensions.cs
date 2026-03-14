namespace Identity.IdentityService.Bootstraping;

/// <summary>
/// Extension methods that register all services required by the Identity Service.
/// Called from <c>Program.cs</c> via <c>builder.AddApplicationServices()</c>.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.Services.AddAuthorization();

        // ── Database ──────────────────────────────────────────────────────────
        // Aspire wires up the connection string from the resource named "DefaultDatabase".
        builder.AddNpgsqlDbContext<ApplicationDbContext>(Consts.DefaultDatabase);

        // ── ASP.NET Core Identity ─────────────────────────────────────────────
        // Identity manages users, passwords, roles, and claims.
        // AddEntityFrameworkStores<T>() stores everything in ApplicationDbContext.
        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Relaxed password rules for development/demo purposes.
                // Tighten these in production!
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ── OpenIddict ────────────────────────────────────────────────────────
        // OpenIddict is the OAuth2 / OpenID Connect server implementation.
        // It is split into three layers:
        //   Core    – abstractions and EF Core stores
        //   Server  – protocol handlers (token issuance, etc.)
        //   Validation – token validation for protected resources
        builder.Services
            .AddOpenIddict()
            .AddCore(options =>
            {
                // Use Entity Framework Core as the storage layer for
                // applications, authorizations, scopes, and tokens.
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();
            })
            .AddServer(options =>
            {
                // ── Endpoints ─────────────────────────────────────────────────
                // These paths are handled by the minimal API handlers in ConnectApi.cs.
                options.SetTokenEndpointUris("/connect/token")
                       .SetAuthorizationEndpointUris("/connect/authorize")
                       .SetEndSessionEndpointUris("/connect/logout")
                       .SetUserInfoEndpointUris("/connect/userinfo");

                // ── Supported grant types ─────────────────────────────────────
                // Authorization Code + PKCE: the recommended flow for user-facing apps.
                // PKCE (Proof Key for Code Exchange) prevents authorization code interception.
                options.AllowAuthorizationCodeFlow()
                       .RequireProofKeyForCodeExchange();

                // Client Credentials: used for machine-to-machine communication
                // (no user is involved; the client authenticates with a secret).
                options.AllowClientCredentialsFlow();

                // Refresh Token: allows obtaining a new access token without re-authentication.
                options.AllowRefreshTokenFlow();

                // ── Credentials ───────────────────────────────────────────────
                if (builder.Environment.IsDevelopment())
                {
                    // Development: use auto-generated, ephemeral certificates.
                    // Do NOT use this in production – tokens won't survive restarts.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Disable access token encryption so tokens can be inspected
                    // with jwt.io during development.
                    options.DisableAccessTokenEncryption();
                }

                // ── ASP.NET Core integration ──────────────────────────────────
                // "Pass-through" mode lets our minimal API handlers (ConnectApi.cs)
                // take control of each endpoint after OpenIddict validates the request.
                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                // Validate tokens issued by the local OpenIddict server instance.
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        // Seed the database with default OAuth2 applications and a test user on startup.
        builder.Services.AddHostedService<OpenIddictSeedWorker>();

        return builder;
    }
}
