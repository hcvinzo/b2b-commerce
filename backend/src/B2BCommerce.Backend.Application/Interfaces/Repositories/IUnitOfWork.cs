namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Unit of Work interface for managing database transactions and repository access
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the product repository
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Gets the order repository
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// Gets the customer repository
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Gets the category repository
    /// </summary>
    ICategoryRepository Categories { get; }

    /// <summary>
    /// Gets the brand repository
    /// </summary>
    IBrandRepository Brands { get; }

    /// <summary>
    /// Gets the payment repository
    /// </summary>
    IPaymentRepository Payments { get; }

    /// <summary>
    /// Gets the shipment repository
    /// </summary>
    IShipmentRepository Shipments { get; }

    /// <summary>
    /// Gets the currency rate repository
    /// </summary>
    ICurrencyRateRepository CurrencyRates { get; }

    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
