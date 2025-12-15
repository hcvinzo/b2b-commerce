using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Payment entity for order payments
/// </summary>
public class Payment : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string PaymentNumber { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Money Amount { get; private set; }

    // Payment gateway information
    public string? TransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // For bank transfers
    public string? BankReferenceNumber { get; private set; }
    public string? BankAccountInfo { get; private set; }

    // Navigation properties
    public Order? Order { get; set; }

    private Payment() // For EF Core
    {
        PaymentNumber = string.Empty;
        Amount = Money.Zero("USD");
    }

    /// <summary>
    /// Creates a new Payment instance
    /// </summary>
    public static Payment Create(Guid orderId, PaymentMethod paymentMethod, Money amount)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            PaymentNumber = GeneratePaymentNumber(),
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            Amount = amount ?? throw new ArgumentNullException(nameof(amount))
        };

        return payment;
    }

    [Obsolete("Use Payment.Create() factory method instead")]
    public Payment(Guid orderId, PaymentMethod paymentMethod, Money amount)
    {
        OrderId = orderId;
        PaymentNumber = GeneratePaymentNumber();
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
    }

    private static string GeneratePaymentNumber()
    {
        return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    public void Authorize(string transactionId, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationDomainException($"Cannot authorize payment with status {Status}");

        Status = PaymentStatus.Authorized;
        TransactionId = transactionId;
        GatewayResponse = gatewayResponse;
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationDomainException($"Cannot capture payment with status {Status}");

        Status = PaymentStatus.Captured;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(string? transactionId = null, string? gatewayResponse = null)
    {
        if (Status == PaymentStatus.Captured)
            throw new InvalidOperationDomainException("Payment is already captured");

        Status = PaymentStatus.Captured;
        PaidAt = DateTime.UtcNow;
        TransactionId = transactionId;
        GatewayResponse = gatewayResponse;
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        GatewayResponse = reason;
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Captured)
            throw new InvalidOperationDomainException("Cannot cancel a captured payment. Use refund instead.");

        Status = PaymentStatus.Cancelled;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationDomainException("Can only refund captured payments");

        Status = PaymentStatus.Refunded;
    }

    public void SetBankTransferDetails(string referenceNumber, string? accountInfo = null)
    {
        BankReferenceNumber = referenceNumber;
        BankAccountInfo = accountInfo;
    }
}
