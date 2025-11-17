namespace B2BCommerce.Backend.Domain.Exceptions;

public class OutOfStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public OutOfStockException(Guid productId, int requestedQuantity, int availableStock)
        : base($"Product {productId} is out of stock. Requested: {requestedQuantity}, Available: {availableStock}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
    }
}
