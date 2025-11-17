namespace B2BCommerce.Backend.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related exceptions
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
