namespace B2BCommerce.Backend.Application.DTOs.Parameters;

/// <summary>
/// DTO for updating a parameter
/// </summary>
public class UpdateParameterDto
{
    public string? Value { get; set; }
    public string? Description { get; set; }
    public bool? IsEditable { get; set; }
}
