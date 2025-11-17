namespace B2BCommerce.Backend.Domain.Exceptions;

public class InsufficientCreditException : DomainException
{
    public decimal RequiredAmount { get; }
    public decimal AvailableCredit { get; }
    public Guid CustomerId { get; }

    public InsufficientCreditException(Guid customerId, decimal requiredAmount, decimal availableCredit)
        : base($"Insufficient credit for customer {customerId}. Required: {requiredAmount}, Available: {availableCredit}")
    {
        CustomerId = customerId;
        RequiredAmount = requiredAmount;
        AvailableCredit = availableCredit;
    }
}
