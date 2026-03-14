namespace Identity.IdentityService.Infrastructure.Entity;

/// <summary>
/// Represents an application user.
/// Extends <see cref="IdentityUser"/> so that custom profile properties
/// can be added in the future without breaking the schema.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Gets or sets the user's display name shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;
}
