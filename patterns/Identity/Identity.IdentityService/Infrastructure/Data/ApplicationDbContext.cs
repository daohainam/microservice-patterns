namespace Identity.IdentityService.Infrastructure.Data;

/// <summary>
/// The application database context.
/// </summary>
/// <remarks>
/// Inherits from <see cref="IdentityDbContext{TUser}"/> which adds all the ASP.NET Core
/// Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, …).
///
/// <see cref="ModelBuilder.UseOpenIddict()"/> registers OpenIddict's own entity sets
/// in the same database:
///   • OpenIddictApplications  – registered OAuth2 clients
///   • OpenIddictAuthorizations – persistent authorization grants
///   • OpenIddictScopes        – API scopes
///   • OpenIddictTokens        – issued tokens (for token revocation)
/// </remarks>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tell OpenIddict to use this DbContext for its own entity sets.
        builder.UseOpenIddict();
    }
}
