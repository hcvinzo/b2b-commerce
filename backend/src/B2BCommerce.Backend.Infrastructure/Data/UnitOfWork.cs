using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Infrastructure.Data.Repositories;
using B2BCommerce.Backend.Infrastructure.Data.Repositories.Integration;
using Microsoft.EntityFrameworkCore.Storage;

namespace B2BCommerce.Backend.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation for transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repository instances
    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private ICustomerRepository? _customers;
    private ICategoryRepository? _categories;
    private IBrandRepository? _brands;
    private IPaymentRepository? _payments;
    private IShipmentRepository? _shipments;
    private ICurrencyRateRepository? _currencyRates;

    // Integration repositories
    private IApiClientRepository? _apiClients;
    private IApiKeyRepository? _apiKeys;
    private IApiKeyUsageLogRepository? _apiKeyUsageLogs;

    // Newsletter repository
    private INewsletterSubscriptionRepository? _newsletterSubscriptions;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public IBrandRepository Brands =>
        _brands ??= new BrandRepository(_context);

    public IPaymentRepository Payments =>
        _payments ??= new PaymentRepository(_context);

    public IShipmentRepository Shipments =>
        _shipments ??= new ShipmentRepository(_context);

    public ICurrencyRateRepository CurrencyRates =>
        _currencyRates ??= new CurrencyRateRepository(_context);

    // Integration repositories
    public IApiClientRepository ApiClients =>
        _apiClients ??= new ApiClientRepository(_context);

    public IApiKeyRepository ApiKeys =>
        _apiKeys ??= new ApiKeyRepository(_context);

    public IApiKeyUsageLogRepository ApiKeyUsageLogs =>
        _apiKeyUsageLogs ??= new ApiKeyUsageLogRepository(_context);

    // Newsletter repository
    public INewsletterSubscriptionRepository NewsletterSubscriptions =>
        _newsletterSubscriptions ??= new NewsletterSubscriptionRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}
