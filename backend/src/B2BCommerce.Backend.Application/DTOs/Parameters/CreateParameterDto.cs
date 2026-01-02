using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Parameters;

/// <summary>
/// DTO for creating a new parameter
/// </summary>
public class CreateParameterDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ParameterType ParameterType { get; set; } = ParameterType.Business;
    public ParameterValueType ValueType { get; set; } = ParameterValueType.String;
    public bool IsEditable { get; set; } = true;
}
