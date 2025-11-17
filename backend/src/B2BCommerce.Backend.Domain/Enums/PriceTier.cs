namespace B2BCommerce.Backend.Domain.Enums;

public enum PriceTier
{
    List = 0,        // Standard list price
    Tier1 = 1,       // First tier (highest discount)
    Tier2 = 2,       // Second tier
    Tier3 = 3,       // Third tier
    Tier4 = 4,       // Fourth tier
    Tier5 = 5,       // Fifth tier (lowest discount)
    Special = 99     // Special pricing (customer-specific)
}
