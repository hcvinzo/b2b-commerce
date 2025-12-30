using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
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
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        return Result<CustomerDto>.Success(MapToCustomerDto(customer));
    }

    public async Task<Result<CustomerDto>> GetByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByTaxNoAsync(taxNo, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        return Result<CustomerDto>.Success(MapToCustomerDto(customer));
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CustomerStatus? status = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            customers = customers.Where(c =>
                c.Title.ToLowerInvariant().Contains(searchLower) ||
                (c.TaxNo != null && c.TaxNo.Contains(searchLower)));
        }

        // Apply active filter (maps to Status == Active)
        if (isActive.HasValue)
        {
            if (isActive.Value)
            {
                customers = customers.Where(c => c.Status == CustomerStatus.Active);
            }
            else
            {
                customers = customers.Where(c => c.Status != CustomerStatus.Active);
            }
        }

        // Apply status filter
        if (status.HasValue)
        {
            customers = customers.Where(c => c.Status == status.Value);
        }

        var totalCount = customers.Count();

        // Apply sorting
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        customers = sortBy?.ToLowerInvariant() switch
        {
            "title" => isDescending
                ? customers.OrderByDescending(c => c.Title)
                : customers.OrderBy(c => c.Title),
            "status" => isDescending
                ? customers.OrderByDescending(c => c.Status)
                : customers.OrderBy(c => c.Status),
            "createdat" => isDescending
                ? customers.OrderByDescending(c => c.CreatedAt)
                : customers.OrderBy(c => c.CreatedAt),
            _ => isDescending
                ? customers.OrderByDescending(c => c.CreatedAt)
                : customers.OrderBy(c => c.Title)
        };

        var pagedCustomers = customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToCustomerDto)
            .ToList();

        var pagedResult = new PagedResult<CustomerDto>(pagedCustomers, pageNumber, pageSize, totalCount);
        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }

    public async Task<Result<IEnumerable<CustomerDto>>> GetPendingCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetPendingCustomersAsync(cancellationToken);
        var customerDtos = customers.Select(MapToCustomerDto);
        return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            customer.Update(
                title: dto.Title,
                taxOffice: dto.TaxOffice,
                taxNo: dto.TaxNo,
                establishmentYear: dto.EstablishmentYear,
                website: dto.Website
            );

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer updated with ID {CustomerId}", customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to update customer", "UPDATE_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        if (customer.Status == CustomerStatus.Active)
        {
            return Result<CustomerDto>.Failure("Customer is already approved", "ALREADY_APPROVED");
        }

        try
        {
            customer.Approve();

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} approved", customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to approve customer", "APPROVAL_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            customer.Reject();

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} rejected", customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to reject customer", "REJECTION_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            customer.Suspend();

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} suspended", customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to suspend customer", "SUSPENSION_FAILED");
        }
    }

    public async Task<Result<CustomerDto>> SetStatusAsync(Guid id, CustomerStatus status, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        try
        {
            customer.SetStatus(status);

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} status set to {Status}", customer.Id, status);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting status for customer {CustomerId}", id);
            return Result<CustomerDto>.Failure("Failed to set customer status", "STATUS_UPDATE_FAILED");
        }
    }

    public async Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        customer.SetStatus(CustomerStatus.Active);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer activated with ID {CustomerId}", id);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        customer.SetStatus(CustomerStatus.Suspended);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer deactivated with ID {CustomerId}", id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
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
            ExternalId = customer.ExternalId,
            ExternalCode = customer.ExternalCode,
            Title = customer.Title,
            TaxOffice = customer.TaxOffice,
            TaxNo = customer.TaxNo,
            EstablishmentYear = customer.EstablishmentYear,
            Website = customer.Website,
            Status = customer.Status.ToString(),
            UserId = customer.UserId,
            DocumentUrls = customer.DocumentUrls,
            Contacts = customer.Contacts?.Select(c => new CustomerContactDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                FullName = $"{c.FirstName} {c.LastName}".Trim(),
                Email = c.Email,
                Position = c.Position,
                DateOfBirth = c.DateOfBirth,
                Gender = c.Gender.ToString(),
                Phone = c.Phone,
                PhoneExt = c.PhoneExt,
                Gsm = c.Gsm,
                IsPrimary = c.IsPrimary,
                IsActive = c.IsActive
            }).ToList() ?? new List<CustomerContactDto>(),
            Addresses = customer.Addresses?.Select(a => new CustomerAddressDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                Title = a.Title,
                FullName = a.FullName,
                AddressType = a.AddressType.ToString(),
                Address = a.Address,
                GeoLocationId = a.GeoLocationId,
                GeoLocationPathName = a.GeoLocation?.PathName,
                PostalCode = a.PostalCode,
                Phone = a.Phone,
                PhoneExt = a.PhoneExt,
                Gsm = a.Gsm,
                TaxNo = a.TaxNo,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive
            }).ToList() ?? new List<CustomerAddressDto>(),
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            LastSyncedAt = customer.LastSyncedAt
        };
    }
}
