using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Customer attribute service implementation
/// </summary>
public class CustomerAttributeService : ICustomerAttributeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerAttributeService> _logger;

    public CustomerAttributeService(IUnitOfWork unitOfWork, ILogger<CustomerAttributeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var attributes = await _unitOfWork.CustomerAttributes.GetByCustomerIdAsync(customerId, cancellationToken);
        return Result<IEnumerable<CustomerAttributeDto>>.Success(attributes.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> GetByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerAttributeType attributeType,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var attributes = await _unitOfWork.CustomerAttributes.GetByCustomerIdAndTypeAsync(
            customerId, attributeType, cancellationToken);
        return Result<IEnumerable<CustomerAttributeDto>>.Success(attributes.Select(MapToDto));
    }

    public async Task<Result<CustomerAttributeDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var attribute = await _unitOfWork.CustomerAttributes.GetByIdAsync(id, cancellationToken);
        if (attribute is null)
        {
            return Result<CustomerAttributeDto>.Failure("Customer attribute not found", "ATTRIBUTE_NOT_FOUND");
        }

        return Result<CustomerAttributeDto>.Success(MapToDto(attribute));
    }

    public async Task<Result<CustomerAttributeDto>> CreateAsync(
        Guid customerId,
        CreateCustomerAttributeDto dto,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerAttributeDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var attribute = CustomerAttribute.Create(
            customerId,
            dto.AttributeType,
            dto.JsonData,
            dto.DisplayOrder);

        await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created customer attribute {AttributeId} of type {AttributeType} for customer {CustomerId}",
            attribute.Id, dto.AttributeType, customerId);

        return Result<CustomerAttributeDto>.Success(MapToDto(attribute));
    }

    public async Task<Result<CustomerAttributeDto>> UpdateAsync(
        Guid id,
        UpdateCustomerAttributeDto dto,
        CancellationToken cancellationToken = default)
    {
        var attribute = await _unitOfWork.CustomerAttributes.GetByIdAsync(id, cancellationToken);
        if (attribute is null)
        {
            return Result<CustomerAttributeDto>.Failure("Customer attribute not found", "ATTRIBUTE_NOT_FOUND");
        }

        attribute.UpdateData(dto.JsonData);
        attribute.UpdateDisplayOrder(dto.DisplayOrder);

        _unitOfWork.CustomerAttributes.Update(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated customer attribute {AttributeId}", id);

        return Result<CustomerAttributeDto>.Success(MapToDto(attribute));
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var attribute = await _unitOfWork.CustomerAttributes.GetByIdAsync(id, cancellationToken);
        if (attribute is null)
        {
            return Result.Failure("Customer attribute not found", "ATTRIBUTE_NOT_FOUND");
        }

        attribute.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer attribute {AttributeId}", id);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> UpsertByTypeAsync(
        Guid customerId,
        UpsertCustomerAttributesByTypeDto dto,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        // Get existing attributes of this type
        var existingAttributes = (await _unitOfWork.CustomerAttributes.GetByCustomerIdAndTypeAsync(
            customerId, dto.AttributeType, cancellationToken)).ToList();

        // Track which existing IDs are in the request
        var requestItemIds = dto.Items
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        // Delete attributes that are no longer in the request
        foreach (var existing in existingAttributes)
        {
            if (!requestItemIds.Contains(existing.Id))
            {
                existing.MarkAsDeleted();
            }
        }

        // Process items: update existing or create new
        var resultAttributes = new List<CustomerAttribute>();
        foreach (var item in dto.Items)
        {
            if (item.Id.HasValue)
            {
                // Update existing
                var existing = existingAttributes.FirstOrDefault(a => a.Id == item.Id.Value);
                if (existing is not null)
                {
                    existing.UpdateData(item.JsonData);
                    existing.UpdateDisplayOrder(item.DisplayOrder);
                    _unitOfWork.CustomerAttributes.Update(existing);
                    resultAttributes.Add(existing);
                }
                else
                {
                    // ID provided but not found - create new with that ID is not supported, create as new
                    var attribute = CustomerAttribute.Create(
                        customerId,
                        dto.AttributeType,
                        item.JsonData,
                        item.DisplayOrder);
                    await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
                    resultAttributes.Add(attribute);
                }
            }
            else
            {
                // Create new
                var attribute = CustomerAttribute.Create(
                    customerId,
                    dto.AttributeType,
                    item.JsonData,
                    item.DisplayOrder);
                await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
                resultAttributes.Add(attribute);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Upserted {Count} customer attributes of type {AttributeType} for customer {CustomerId}",
            resultAttributes.Count, dto.AttributeType, customerId);

        return Result<IEnumerable<CustomerAttributeDto>>.Success(resultAttributes.Select(MapToDto));
    }

    private static CustomerAttributeDto MapToDto(CustomerAttribute attribute)
    {
        return new CustomerAttributeDto
        {
            Id = attribute.Id,
            CustomerId = attribute.CustomerId,
            AttributeType = attribute.AttributeType,
            AttributeTypeName = attribute.AttributeType.ToString(),
            DisplayOrder = attribute.DisplayOrder,
            JsonData = attribute.JsonData,
            CreatedAt = attribute.CreatedAt,
            UpdatedAt = attribute.UpdatedAt
        };
    }
}
