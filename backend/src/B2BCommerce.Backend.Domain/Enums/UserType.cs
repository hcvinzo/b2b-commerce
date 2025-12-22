namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Defines the type of user in the system
/// </summary>
public enum UserType
{
    /// <summary>
    /// System administrators and business users with admin panel access
    /// </summary>
    Admin = 0,

    /// <summary>
    /// Dealers/Frontend users with customer portal access
    /// </summary>
    Customer = 1,

    /// <summary>
    /// System-generated users for API client authentication
    /// </summary>
    ApiClient = 2
}
