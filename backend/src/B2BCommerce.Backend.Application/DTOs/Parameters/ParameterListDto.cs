using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Parameters;

/// <summary>
/// DTO for parameter list view
/// </summary>
public class ParameterListDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public ParameterType ParameterType { get; set; }
    public ParameterValueType ValueType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
