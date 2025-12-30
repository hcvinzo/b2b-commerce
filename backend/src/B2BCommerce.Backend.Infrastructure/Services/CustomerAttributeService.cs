using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
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

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> GetByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var attribute = await _unitOfWork.CustomerAttributes.GetByCustomerIdAndDefinitionIdAsync(
            customerId, attributeDefinitionId, cancellationToken);

        if (attribute is null)
        {
            return Result<IEnumerable<CustomerAttributeDto>>.Success(Enumerable.Empty<CustomerAttributeDto>());
        }

        return Result<IEnumerable<CustomerAttributeDto>>.Success(new[] { MapToDto(attribute) });
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

        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(dto.AttributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            return Result<CustomerAttributeDto>.Failure("Attribute definition not found", "ATTRIBUTE_DEFINITION_NOT_FOUND");
        }

        var attribute = CustomerAttribute.Create(customerId, dto.AttributeDefinitionId, dto.Value);

        await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created customer attribute {AttributeId} with definition {AttributeDefinitionId} for customer {CustomerId}",
            attribute.Id, dto.AttributeDefinitionId, customerId);

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

        attribute.UpdateValue(dto.Value);

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

        _unitOfWork.CustomerAttributes.Remove(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer attribute {AttributeId}", id);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> UpsertByDefinitionAsync(
        Guid customerId,
        UpsertCustomerAttributesByDefinitionDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "UpsertByDefinitionAsync called for customer {CustomerId}, definition {AttributeDefinitionId}",
            customerId, dto.AttributeDefinitionId);

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", customerId);
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        var attributeDefinition = await _unitOfWork.AttributeDefinitions.GetByIdAsync(dto.AttributeDefinitionId, cancellationToken);
        if (attributeDefinition is null)
        {
            _logger.LogWarning(
                "Attribute definition {AttributeDefinitionId} not found for customer {CustomerId}",
                dto.AttributeDefinitionId, customerId);
            return Result<IEnumerable<CustomerAttributeDto>>.Failure("Attribute definition not found", "ATTRIBUTE_DEFINITION_NOT_FOUND");
        }

        _logger.LogDebug(
            "Found attribute definition {AttributeDefinitionId} (Code: {Code}) for customer {CustomerId}",
            dto.AttributeDefinitionId, attributeDefinition.Code, customerId);

        // Delete existing attribute for this definition (replace semantics)
        await _unitOfWork.CustomerAttributes.DeleteByCustomerIdAndDefinitionIdAsync(
            customerId, dto.AttributeDefinitionId, cancellationToken);

        // Process items: create new attributes
        var resultAttributes = new List<CustomerAttribute>();
        foreach (var item in dto.Items)
        {
            var attribute = CustomerAttribute.Create(customerId, dto.AttributeDefinitionId, item.Value);
            await _unitOfWork.CustomerAttributes.AddAsync(attribute, cancellationToken);
            resultAttributes.Add(attribute);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Upserted {Count} customer attributes for definition {AttributeDefinitionId} for customer {CustomerId}",
            resultAttributes.Count, dto.AttributeDefinitionId, customerId);

        return Result<IEnumerable<CustomerAttributeDto>>.Success(resultAttributes.Select(MapToDto));
    }

    private static CustomerAttributeDto MapToDto(CustomerAttribute attribute)
    {
        return new CustomerAttributeDto
        {
            Id = attribute.Id,
            CustomerId = attribute.CustomerId,
            AttributeDefinitionId = attribute.AttributeDefinitionId,
            AttributeCode = attribute.AttributeDefinition?.Code ?? string.Empty,
            AttributeName = attribute.AttributeDefinition?.Name ?? string.Empty,
            Value = attribute.Value,
            CreatedAt = attribute.CreatedAt,
            UpdatedAt = attribute.UpdatedAt
        };
    }
}
