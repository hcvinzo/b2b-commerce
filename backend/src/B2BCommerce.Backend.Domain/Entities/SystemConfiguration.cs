using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// System configuration entity for application settings
/// </summary>
public class SystemConfiguration : BaseEntity
{
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string Category { get; private set; }
    public string Description { get; private set; }
    public bool IsEditable { get; private set; }

    private SystemConfiguration() // For EF Core
    {
        Key = string.Empty;
        Value = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
    }

    public SystemConfiguration(string key, string value, string category, string description, bool isEditable = true)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be null or empty", nameof(key));

        Key = key;
        Value = value ?? string.Empty;
        Category = category ?? "General";
        Description = description ?? string.Empty;
        IsEditable = isEditable;
    }

    public void UpdateValue(string newValue)
    {
        if (!IsEditable)
            throw new InvalidOperationException($"Configuration key '{Key}' is not editable");

        Value = newValue ?? string.Empty;
    }
}
