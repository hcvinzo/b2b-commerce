namespace B2BCommerce.Backend.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string entityName, object key, Exception innerException)
        : base($"{entityName} with key '{key}' was not found.", innerException)
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }
}
