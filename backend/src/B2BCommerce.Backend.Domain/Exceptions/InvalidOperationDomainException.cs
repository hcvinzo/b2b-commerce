namespace B2BCommerce.Backend.Domain.Exceptions;

public class InvalidOperationDomainException : DomainException
{
    public InvalidOperationDomainException(string message) : base(message)
    {
    }

    public InvalidOperationDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
