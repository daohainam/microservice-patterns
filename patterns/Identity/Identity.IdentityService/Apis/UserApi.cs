namespace Identity.IdentityService.Apis;

/// <summary>
/// Maps user-management endpoints.
///
/// Endpoints:
///   POST /api/identity/v1/users/register  – create a new account
///   POST /api/identity/v1/users/login     – issue an Identity cookie (for the authorize flow)
///   POST /api/identity/v1/users/logout    – clear the Identity cookie
/// </summary>
public static class UserApi
{
    public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity/v1/users")
                       .WithTags("User Management");

        group.MapPost("register", RegisterAsync)
             .WithSummary("Register a new user")
             .WithDescription("Creates a new user account with the provided email and password.");

        group.MapPost("login", LoginAsync)
             .WithSummary("Log in")
             .WithDescription("""
                 Validates credentials and issues an Identity cookie.
                 Used by the authorization endpoint to confirm the user's identity
                 before issuing an authorization code.
                 """);

        group.MapPost("logout", LogoutAsync)
             .WithSummary("Log out")
             .WithDescription("Clears the Identity cookie.");

        return app;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    private static async Task<Results<Ok<UserResponse>, ValidationProblem>> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName ?? request.Email,
            EmailConfirmed = true, // Skip e-mail verification for simplicity.
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        }

        await userManager.AddToRoleAsync(user, "user");

        return TypedResults.Ok(new UserResponse(
            Id: await userManager.GetUserIdAsync(user),
            Email: request.Email,
            DisplayName: user.DisplayName));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    private static async Task<Results<Ok, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        // isPersistent: false – the cookie expires when the browser is closed.
        var result = await signInManager.PasswordSignInAsync(
            user, request.Password, isPersistent: false, lockoutOnFailure: false);

        return result.Succeeded ? TypedResults.Ok() : TypedResults.Unauthorized();
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    private static async Task<Ok> LogoutAsync(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();

        return TypedResults.Ok();
    }
}

// ── Request / response models ─────────────────────────────────────────────────

/// <summary>Payload for <c>POST /api/identity/v1/users/register</c>.</summary>
/// <param name="Email">The user's e-mail address (also used as the username).</param>
/// <param name="Password">Plain-text password. Will be hashed by Identity.</param>
/// <param name="DisplayName">Optional display name. Defaults to the e-mail address.</param>
public record RegisterRequest(string Email, string Password, string? DisplayName = null);

/// <summary>Payload for <c>POST /api/identity/v1/users/login</c>.</summary>
/// <param name="Email">The user's e-mail address.</param>
/// <param name="Password">Plain-text password.</param>
public record LoginRequest(string Email, string Password);

/// <summary>Response returned after a successful registration.</summary>
/// <param name="Id">The new user's identifier.</param>
/// <param name="Email">The registered e-mail address.</param>
/// <param name="DisplayName">The display name.</param>
public record UserResponse(string Id, string Email, string DisplayName);
