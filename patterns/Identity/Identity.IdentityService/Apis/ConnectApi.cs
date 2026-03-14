using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace Identity.IdentityService.Apis;

/// <summary>
/// Maps the OAuth2 / OpenID Connect protocol endpoints:
///   POST  /connect/token      – issue access tokens, id tokens, and refresh tokens
///   GET   /connect/authorize  – start the Authorization Code flow (user login + consent)
///   POST  /connect/logout     – end the user's session
///   GET   /connect/userinfo   – return claims about the authenticated user
///
/// These handlers run in OpenIddict's "pass-through" mode: OpenIddict validates the
/// incoming request (client ID, grant type, PKCE, etc.) and then hands control to
/// the handler below to build the ClaimsPrincipal that will be encoded into the tokens.
/// </summary>
public static class ConnectApi
{
    public static IEndpointRouteBuilder MapConnectApi(this IEndpointRouteBuilder app)
    {
        // Token endpoint – the client exchanges a credential (code, secret, refresh token)
        // for one or more tokens (access token, id token, refresh token).
        app.MapPost("/connect/token", HandleTokenAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .WithSummary("Token endpoint")
           .WithDescription("""
               Issues access tokens using the following grant types:
               • client_credentials – service-to-service (no user)
               • authorization_code  – exchange a code from the authorization endpoint
               • refresh_token       – exchange a refresh token for a new access token
               """)
           .ExcludeFromDescription(); // Don't expose internal OAuth2 plumbing in OpenAPI

        // Authorization endpoint – redirects the user's browser to this endpoint.
        // OpenIddict validates the request and, if the user is not logged in,
        // the handler below challenges the Identity cookie scheme so the browser
        // is redirected to the login page.
        app.MapGet("/connect/authorize", HandleAuthorizationAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .ExcludeFromDescription();

        app.MapPost("/connect/authorize", HandleAuthorizationAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .ExcludeFromDescription();

        // Logout endpoint – clears the Identity cookie and redirects the browser
        // back to the post_logout_redirect_uri registered by the client.
        app.MapGet("/connect/logout", HandleLogoutAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .ExcludeFromDescription();

        app.MapPost("/connect/logout", HandleLogoutAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .ExcludeFromDescription();

        // UserInfo endpoint – protected by the access token.
        // Returns a JSON object with claims about the current user.
        app.MapGet("/connect/userinfo", HandleUserinfoAsync)
           .WithTags("OAuth2 / OpenID Connect")
           .ExcludeFromDescription()
           .RequireAuthorization();

        return app;
    }

    // ── Token endpoint ────────────────────────────────────────────────────────

    private static async Task<IResult> HandleTokenAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager)
    {
        // OpenIddict has already validated the request (client ID, grant type, …).
        // Retrieve the validated request object from the HTTP context.
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // ── Client Credentials ────────────────────────────────────────────────
        // The client authenticates itself (no user involved).
        // Typically used by background services, schedulers, and microservices.
        if (request.IsClientCredentialsGrantType())
        {
            var application = await applicationManager.FindByClientIdAsync(request.ClientId!)
                ?? throw new InvalidOperationException("The application cannot be found.");

            // Build a ClaimsIdentity that will be encoded into the access token.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Use the client_id as the subject identifier.
            identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

            // Set the scopes that were requested (and are permitted for this client).
            identity.SetScopes(request.GetScopes());
            identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            // Specify where each claim should appear (access token, identity token).
            identity.SetDestinations(GetDestinations);

            return Results.SignIn(new ClaimsPrincipal(identity),
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // ── Authorization Code ────────────────────────────────────────────────
        // The client exchanges the short-lived authorization code (obtained from
        // the /connect/authorize endpoint) for an access token + refresh token.
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // OpenIddict has already validated the code / refresh token.
            // Retrieve the ClaimsPrincipal that was stored when the code was issued.
            var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var userId = result.Principal?.GetClaim(Claims.Subject)
                ?? throw new InvalidOperationException("The user identifier cannot be retrieved.");

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The token is no longer valid.",
                    }));
            }

            // Ensure the user is still allowed to sign in (not locked out, etc.).
            if (!await signInManager.CanSignInAsync(user))
            {
                return Results.Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The user is no longer allowed to sign in.",
                    }));
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Update the identity with fresh claims from the database so that
            // changes to the user's profile or roles are reflected immediately.
            identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, user.DisplayName)
                    .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

            identity.SetDestinations(GetDestinations);

            return Results.SignIn(new ClaimsPrincipal(identity),
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    // ── Authorization endpoint ────────────────────────────────────────────────

    private static async Task<IResult> HandleAuthorizationAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user session from the Identity cookie.
        var result = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user is not authenticated, challenge the Identity scheme so the
        // browser is redirected to the login page (mapped in UserApi.cs).
        // The current URL is preserved so the user lands back here after login.
        if (!result.Succeeded)
        {
            return Results.Challenge(
                authenticationSchemes: [IdentityConstants.ApplicationScheme],
                properties: new AuthenticationProperties
                {
                    RedirectUri = context.Request.PathBase + context.Request.Path
                        + QueryString.Create(
                            context.Request.HasFormContentType
                                ? [.. context.Request.Form]
                                : [.. context.Request.Query]),
                });
        }

        var user = await userManager.GetUserAsync(result.Principal!)
            ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        // Build the identity for the authorization code (the actual access token
        // is built later in HandleTokenAsync when the code is exchanged).
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, user.DisplayName)
                .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        // Approve all requested scopes. In a production system you would
        // present a consent screen to the user here.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);

        return Results.SignIn(new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ── Logout endpoint ───────────────────────────────────────────────────────

    private static async Task<IResult> HandleLogoutAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager)
    {
        // Delete the Identity cookie. OpenIddict will handle the redirect to
        // the post_logout_redirect_uri registered by the client.
        await signInManager.SignOutAsync();

        return Results.SignIn(new ClaimsPrincipal(new ClaimsIdentity()),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ── UserInfo endpoint ─────────────────────────────────────────────────────

    private static async Task<IResult> HandleUserinfoAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(context.User)
            ?? throw new InvalidOperationException("The user profile cannot be retrieved.");

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = await userManager.GetUserIdAsync(user),
        };

        if (context.User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = await userManager.GetEmailAsync(user) ?? string.Empty;
            claims[Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (context.User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = user.DisplayName;
            claims[Claims.PreferredUsername] = await userManager.GetUserNameAsync(user) ?? string.Empty;
        }

        if (context.User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = await userManager.GetRolesAsync(user);
        }

        return Results.Ok(claims);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines which token types (access token, identity token) should
    /// contain each claim.
    ///
    /// • Subject and Name are included in both token types.
    /// • Role is included in the access token only (to keep id tokens small).
    /// • All other claims go into the access token only.
    /// </summary>
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or Claims.Subject
                => [Destinations.AccessToken, Destinations.IdentityToken],

            Claims.Email or Claims.EmailVerified
                when claim.Subject?.HasScope(Scopes.Email) == true
                => [Destinations.AccessToken, Destinations.IdentityToken],

            Claims.Role
                when claim.Subject?.HasScope(Scopes.Roles) == true
                => [Destinations.AccessToken, Destinations.IdentityToken],

            _ => [Destinations.AccessToken],
        };
    }
}
