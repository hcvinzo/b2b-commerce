using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Customer service implementation
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        return Result<CustomerDto>.Success(MapToCustomerDto(customer));
    }

    public async Task<Result<CustomerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);
        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        return Result<CustomerDto>.Success(MapToCustomerDto(customer));
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        var totalCount = customers.Count();

        var pagedCustomers = customers
            .OrderBy(c => c.CompanyName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToCustomerDto)
            .ToList();

        var pagedResult = new PagedResult<CustomerDto>(pagedCustomers, pageNumber, pageSize, totalCount);
        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }

    public async Task<Result<IEnumerable<CustomerDto>>> GetUnapprovedCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetUnapprovedCustomersAsync(cancellationToken);
        var customerDtos = customers.Select(MapToCustomerDto);
        return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            // Update contact info
            customer.UpdateContactInfo(
                companyName: dto.CompanyName,
                contactPersonName: dto.ContactPersonName,
                contactPersonTitle: dto.ContactPersonTitle,
                phone: new PhoneNumber(dto.Phone)
            );

            // Update billing address
            customer.UpdateBillingAddress(new Address(
                dto.BillingStreet,
                dto.BillingCity,
                dto.BillingState,
                dto.BillingCountry,
                dto.BillingPostalCode
            ));

            // Update shipping address
            customer.UpdateShippingAddress(new Address(
                dto.ShippingStreet,
                dto.ShippingCity,
                dto.ShippingState,
                dto.ShippingCountry,
                dto.ShippingPostalCode
            ));

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer updated with ID {CustomerId}", customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating customer");
            return Result<CustomerDto>.Failure(ex.Message, "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to update customer", "UPDATE_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> ApproveAsync(Guid id, string approvedBy, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        if (customer.IsApproved)
        {
            return Result<CustomerDto>.Failure("Customer is already approved", "ALREADY_APPROVED");
        }

        try
        {
            customer.Approve(approvedBy);

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} approved by {ApprovedBy}", customer.Id, approvedBy);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to approve customer", "APPROVAL_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            var newCreditMoney = new Money(newCreditLimit, customer.CreditLimit.Currency);
            customer.UpdateCreditLimit(newCreditMoney);

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} credit limit updated to {CreditLimit}",
                customer.Id, newCreditLimit);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit limit for customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to update credit limit", "UPDATE_FAILED");
        }
    }

    public async Task<Result<decimal>> GetAvailableCreditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<decimal>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var availableCredit = customer.GetAvailableCredit();
        return Result<decimal>.Success(availableCredit.Amount);
    }

    public async Task<Result<bool>> IsCreditNearLimitAsync(Guid id, decimal thresholdPercentage = 90, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result<bool>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        // Convert percentage (90) to decimal ratio (0.9)
        var isNearLimit = customer.IsCreditNearLimit(thresholdPercentage / 100);
        return Result<bool>.Success(isNearLimit);
    }

    public async Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        customer.Activate();
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer activated with ID {CustomerId}", id);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        customer.Deactivate();
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer deactivated with ID {CustomerId}", id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        _unitOfWork.Customers.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer deleted with ID {CustomerId}", id);

        return Result.Success();
    }

    private static CustomerDto MapToCustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            TaxNumber = customer.TaxNumber.Value,
            Email = customer.Email.Value,
            Phone = customer.Phone.Value,
            Type = customer.Type.ToString(),
            PriceTier = customer.PriceTier.ToString(),
            CreditLimit = customer.CreditLimit.Amount,
            UsedCredit = customer.UsedCredit.Amount,
            AvailableCredit = customer.GetAvailableCredit().Amount,
            Currency = customer.CreditLimit.Currency,
            IsApproved = customer.IsApproved,
            ApprovedAt = customer.ApprovedAt,
            ApprovedBy = customer.ApprovedBy,
            ContactPersonName = customer.ContactPersonName,
            ContactPersonTitle = customer.ContactPersonTitle,
            BillingStreet = customer.BillingAddress.Street,
            BillingCity = customer.BillingAddress.City,
            BillingState = customer.BillingAddress.State,
            BillingCountry = customer.BillingAddress.Country,
            BillingPostalCode = customer.BillingAddress.PostalCode,
            ShippingStreet = customer.ShippingAddress.Street,
            ShippingCity = customer.ShippingAddress.City,
            ShippingState = customer.ShippingAddress.State,
            ShippingCountry = customer.ShippingAddress.Country,
            ShippingPostalCode = customer.ShippingAddress.PostalCode,
            PreferredCurrency = customer.PreferredCurrency,
            PreferredLanguage = customer.PreferredLanguage,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt ?? customer.CreatedAt
        };
    }
}
