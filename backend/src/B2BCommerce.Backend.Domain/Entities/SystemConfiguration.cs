using System.Globalization;
using System.Text.Json;
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// System configuration entity for application settings and parameters
/// </summary>
public class SystemConfiguration : BaseEntity
{
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string Category { get; private set; }
    public string Description { get; private set; }
    public bool IsEditable { get; private set; }
    public ParameterType ParameterType { get; private set; }
    public ParameterValueType ValueType { get; private set; }

    private SystemConfiguration() // For EF Core
    {
        Key = string.Empty;
        Value = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
    }

    public SystemConfiguration(
        string key,
        string value,
        string category,
        string description,
        ParameterType parameterType = ParameterType.System,
        ParameterValueType valueType = ParameterValueType.String,
        bool isEditable = true)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Configuration key cannot be null or empty", nameof(key));
        }

        Key = key;
        Category = ExtractCategory(key, category);
        Description = description ?? string.Empty;
        ParameterType = parameterType;
        ValueType = valueType;
        IsEditable = isEditable;

        // Validate and set value
        ValidateValue(value, valueType);
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Factory method for creating a new configuration parameter
    /// </summary>
    public static SystemConfiguration Create(
        string key,
        string value,
        string? description = null,
        ParameterType parameterType = ParameterType.System,
        ParameterValueType valueType = ParameterValueType.String,
        bool isEditable = true)
    {
        return new SystemConfiguration(
            key,
            value,
            ExtractCategoryFromKey(key),
            description ?? string.Empty,
            parameterType,
            valueType,
            isEditable);
    }

    /// <summary>
    /// Updates the configuration metadata (description and editability)
    /// </summary>
    public void Update(string? description, bool? isEditable)
    {
        if (description is not null)
        {
            Description = description;
        }

        if (isEditable.HasValue)
        {
            IsEditable = isEditable.Value;
        }
    }

    /// <summary>
    /// Updates the configuration value with validation
    /// </summary>
    public void UpdateValue(string newValue)
    {
        if (!IsEditable)
        {
            throw new DomainException($"Configuration key '{Key}' is not editable");
        }

        ValidateValue(newValue, ValueType);
        Value = newValue ?? string.Empty;
    }

    /// <summary>
    /// Gets the value as a typed object
    /// </summary>
    public T? GetTypedValue<T>()
    {
        if (string.IsNullOrEmpty(Value))
        {
            return default;
        }

        return ValueType switch
        {
            ParameterValueType.String => (T)(object)Value,
            ParameterValueType.Number => ParseNumber<T>(Value),
            ParameterValueType.Boolean => (T)(object)bool.Parse(Value),
            ParameterValueType.DateTime => (T)(object)DateTime.Parse(Value, CultureInfo.InvariantCulture),
            ParameterValueType.Json => JsonSerializer.Deserialize<T>(Value),
            _ => default
        };
    }

    private static T ParseNumber<T>(string value)
    {
        var type = typeof(T);
        if (type == typeof(int))
        {
            return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
        }
        if (type == typeof(long))
        {
            return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
        }
        if (type == typeof(decimal))
        {
            return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
        }
        if (type == typeof(double))
        {
            return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
        }
        if (type == typeof(float))
        {
            return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Unsupported number type: {type.Name}");
    }

    private static void ValidateValue(string? value, ParameterValueType valueType)
    {
        if (string.IsNullOrEmpty(value))
        {
            return; // Empty values are allowed
        }

        switch (valueType)
        {
            case ParameterValueType.Number:
                if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    throw new DomainException($"Value '{value}' is not a valid number");
                }
                break;

            case ParameterValueType.Boolean:
                if (!bool.TryParse(value, out _))
                {
                    throw new DomainException($"Value '{value}' is not a valid boolean. Use 'true' or 'false'");
                }
                break;

            case ParameterValueType.DateTime:
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    throw new DomainException($"Value '{value}' is not a valid date/time");
                }
                break;

            case ParameterValueType.Json:
                try
                {
                    using var doc = JsonDocument.Parse(value);
                }
                catch (JsonException)
                {
                    throw new DomainException($"Value is not valid JSON");
                }
                break;

            case ParameterValueType.String:
            default:
                // String values are always valid
                break;
        }
    }

    private static string ExtractCategoryFromKey(string key)
    {
        // Extract category from key format: "category.subcategory.name" -> "category"
        var dotIndex = key.IndexOf('.');
        return dotIndex > 0 ? key[..dotIndex] : "general";
    }

    private static string ExtractCategory(string key, string? explicitCategory)
    {
        if (!string.IsNullOrWhiteSpace(explicitCategory))
        {
            return explicitCategory;
        }

        return ExtractCategoryFromKey(key);
    }
}
