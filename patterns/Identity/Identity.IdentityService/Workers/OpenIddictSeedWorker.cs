namespace Identity.IdentityService.Workers;

/// <summary>
/// A hosted service that runs once at startup to seed the database with
/// default OAuth2 applications (clients) and a demo user.
///
/// In a real system you would register clients via an admin API or a
/// configuration file. This worker exists purely for educational clarity.
/// </summary>
public sealed class OpenIddictSeedWorker(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        await SeedApplicationsAsync(scope.ServiceProvider, cancellationToken);
        await SeedUsersAsync(scope.ServiceProvider, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // ── OAuth2 Applications ───────────────────────────────────────────────────

    /// <summary>
    /// Registers the default OAuth2 client applications with OpenIddict.
    /// Each application entry represents a trusted client that is allowed to
    /// request tokens from this authorization server.
    /// </summary>
    private static async Task SeedApplicationsAsync(
        IServiceProvider services, CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        // ── Machine-to-machine client (Client Credentials flow) ───────────────
        // Use this client when one microservice needs to call another
        // without involving a user (e.g. a background worker or a scheduler).
        if (await manager.FindByClientIdAsync("service-client", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "service-client",
                ClientSecret = "service-client-secret",

                // DisplayName is shown in the Aspire dashboard and admin UIs.
                DisplayName = "Service-to-Service Client",

                // Client credentials clients must not have redirect URIs.
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "api",
                },
            }, cancellationToken);
        }

        // ── Interactive client (Authorization Code + PKCE flow) ───────────────
        // Use this client when a user-facing application (SPA, mobile app, …)
        // needs to authenticate users and obtain tokens on their behalf.
        if (await manager.FindByClientIdAsync("web-client", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "web-client",

                // Public clients (SPAs, native apps) do NOT use a client secret
                // because secrets cannot be kept confidential in those environments.
                // PKCE takes over the role of the client secret.
                ClientType = ClientTypes.Public,

                DisplayName = "Web / SPA Client",

                // After login the user is redirected back to this URI with the
                // authorization code. Adjust for your front-end application.
                RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") },
                PostLogoutRedirectUris = { new Uri("https://oauth.pstmn.io/v1/signout") },

                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + "api",
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                },
            }, cancellationToken);
        }

        // ── API Scope ─────────────────────────────────────────────────────────
        // Scopes model what a token is allowed to access. Other services can
        // introspect the token and check for the "api" scope before granting access.
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("api", cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "API access",
                Resources = { "resource-server" },
            }, cancellationToken);
        }
    }

    // ── Demo Users ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a demo admin user and a demo regular user for local testing.
    /// Never seed hard-coded credentials in a production system.
    /// </summary>
    private static async Task SeedUsersAsync(
        IServiceProvider services, CancellationToken cancellationToken)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure the "admin" role exists.
        if (!await roleManager.RoleExistsAsync("admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("admin"));
        }

        // Ensure the "user" role exists.
        if (!await roleManager.RoleExistsAsync("user"))
        {
            await roleManager.CreateAsync(new IdentityRole("user"));
        }

        // Create the demo admin account.
        const string adminEmail = "admin@example.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Demo Admin",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "admin");
            await userManager.AddToRoleAsync(admin, "user");
        }

        // Create the demo regular user account.
        const string userEmail = "user@example.com";
        if (await userManager.FindByEmailAsync(userEmail) is null)
        {
            var user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                DisplayName = "Demo User",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user, "User123!");
            await userManager.AddToRoleAsync(user, "user");
        }
    }
}
