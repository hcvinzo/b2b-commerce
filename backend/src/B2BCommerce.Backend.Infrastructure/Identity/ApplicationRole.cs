using Microsoft.AspNetCore.Identity;

namespace B2BCommerce.Backend.Infrastructure.Identity;

/// <summary>
/// Application role extending IdentityRole
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ApplicationRole()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public ApplicationRole(string roleName, string? description = null) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
        Description = description;
    }
}
