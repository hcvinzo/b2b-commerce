namespace B2BCommerce.Backend.Application.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., concurrent modification, duplicate entry)
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base("A conflict occurred while processing the request.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string entityName, object key)
        : base($"A conflict occurred for {entityName} with key '{key}'.")
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
