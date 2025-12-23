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

        // Delete existing attributes of this type
        await _unitOfWork.CustomerAttributes.DeleteByCustomerIdAndTypeAsync(
            customerId, dto.AttributeType, cancellationToken);

        // Create new attributes
        var newAttributes = new List<CustomerAttribute>();
        foreach (var item in dto.Items)
        {
            var attribute = CustomerAttribute.Create(
                customerId,
                dto.AttributeType,
                item.JsonData,
                item.DisplayOrder);

            newAttributes.Add(attribute);
            await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Upserted {Count} customer attributes of type {AttributeType} for customer {CustomerId}",
            newAttributes.Count, dto.AttributeType, customerId);

        return Result<IEnumerable<CustomerAttributeDto>>.Success(newAttributes.Select(MapToDto));
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
