using B2BCommerce.Backend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace B2BCommerce.Backend.Infrastructure.Identity;

/// <summary>
/// Application role extending IdentityRole
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The type of user this role is intended for.
    /// Roles can only be assigned to users of matching UserType.
    /// </summary>
    public UserType UserType { get; set; } = UserType.Admin;

    public ApplicationRole()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public ApplicationRole(string roleName, string? description = null, UserType userType = UserType.Admin) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
        Description = description;
        UserType = userType;
    }
}
