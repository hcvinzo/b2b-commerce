using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Currencies;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Currency service implementation
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        IUnitOfWork unitOfWork,
        ILogger<CurrencyService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<CurrencyListDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _unitOfWork.Currencies.GetAllOrderedAsync(cancellationToken);
        var dtos = currencies.Select(MapToListDto).ToList();
        return Result<List<CurrencyListDto>>.Success(dtos);
    }

    public async Task<Result<List<CurrencyListDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _unitOfWork.Currencies.GetActiveAsync(cancellationToken);
        var dtos = currencies.Select(MapToListDto).ToList();
        return Result<List<CurrencyListDto>>.Success(dtos);
    }

    public async Task<Result<CurrencyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

        if (currency is null)
        {
            return Result<CurrencyDto>.Failure($"Currency with ID {id} not found", "NOT_FOUND");
        }

        return Result<CurrencyDto>.Success(MapToDto(currency));
    }

    public async Task<Result<CurrencyDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var currency = await _unitOfWork.Currencies.GetByCodeAsync(code, cancellationToken);

        if (currency is null)
        {
            return Result<CurrencyDto>.Failure($"Currency with code '{code}' not found", "NOT_FOUND");
        }

        return Result<CurrencyDto>.Success(MapToDto(currency));
    }

    public async Task<Result<CurrencyDto>> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var currency = await _unitOfWork.Currencies.GetDefaultAsync(cancellationToken);

        if (currency is null)
        {
            return Result<CurrencyDto>.Failure("No default currency is set", "NOT_FOUND");
        }

        return Result<CurrencyDto>.Success(MapToDto(currency));
    }

    public async Task<Result<CurrencyDto>> CreateAsync(CreateCurrencyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if currency code already exists
            if (await _unitOfWork.Currencies.ExistsByCodeAsync(dto.Code, cancellationToken))
            {
                return Result<CurrencyDto>.Failure($"Currency with code '{dto.Code.ToUpperInvariant()}' already exists", "DUPLICATE");
            }

            var currency = Currency.Create(
                dto.Code,
                dto.Name,
                dto.Symbol,
                dto.DecimalPlaces,
                dto.DisplayOrder,
                dto.RateManagementMode);

            await _unitOfWork.Currencies.AddAsync(currency, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency created: {CurrencyCode} - {CurrencyName}", currency.Code, currency.Name);

            return Result<CurrencyDto>.Success(MapToDto(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating currency: {CurrencyCode}", dto.Code);
            return Result<CurrencyDto>.Failure($"Error creating currency: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyDto>> UpdateAsync(Guid id, UpdateCurrencyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

            if (currency is null)
            {
                return Result<CurrencyDto>.Failure($"Currency with ID {id} not found", "NOT_FOUND");
            }

            currency.Update(dto.Name, dto.Symbol, dto.DecimalPlaces, dto.DisplayOrder, dto.RateManagementMode);

            _unitOfWork.Currencies.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency updated: {CurrencyCode} - {CurrencyName}", currency.Code, currency.Name);

            return Result<CurrencyDto>.Success(MapToDto(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency: {CurrencyId}", id);
            return Result<CurrencyDto>.Failure($"Error updating currency: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

            if (currency is null)
            {
                return Result.Failure($"Currency with ID {id} not found", "NOT_FOUND");
            }

            if (currency.IsDefault)
            {
                return Result.Failure("Cannot delete the default currency", "VALIDATION");
            }

            currency.MarkAsDeleted();
            _unitOfWork.Currencies.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency deleted: {CurrencyCode}", currency.Code);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting currency: {CurrencyId}", id);
            return Result.Failure($"Error deleting currency: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

            if (currency is null)
            {
                return Result<CurrencyDto>.Failure($"Currency with ID {id} not found", "NOT_FOUND");
            }

            currency.Activate();
            _unitOfWork.Currencies.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency activated: {CurrencyCode}", currency.Code);

            return Result<CurrencyDto>.Success(MapToDto(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating currency: {CurrencyId}", id);
            return Result<CurrencyDto>.Failure($"Error activating currency: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

            if (currency is null)
            {
                return Result<CurrencyDto>.Failure($"Currency with ID {id} not found", "NOT_FOUND");
            }

            currency.Deactivate();
            _unitOfWork.Currencies.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency deactivated: {CurrencyCode}", currency.Code);

            return Result<CurrencyDto>.Success(MapToDto(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating currency: {CurrencyId}", id);
            return Result<CurrencyDto>.Failure($"Error deactivating currency: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyDto>> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(id, cancellationToken);

            if (currency is null)
            {
                return Result<CurrencyDto>.Failure($"Currency with ID {id} not found", "NOT_FOUND");
            }

            if (!currency.IsActive)
            {
                return Result<CurrencyDto>.Failure("Cannot set inactive currency as default", "VALIDATION");
            }

            // Clear existing defaults
            await _unitOfWork.Currencies.ClearAllDefaultsAsync(cancellationToken);

            // Set new default
            currency.SetAsDefault();
            _unitOfWork.Currencies.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency set as default: {CurrencyCode}", currency.Code);

            return Result<CurrencyDto>.Success(MapToDto(currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default currency: {CurrencyId}", id);
            return Result<CurrencyDto>.Failure($"Error setting default currency: {ex.Message}");
        }
    }

    private static CurrencyDto MapToDto(Currency currency)
    {
        return new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            DecimalPlaces = currency.DecimalPlaces,
            IsDefault = currency.IsDefault,
            IsActive = currency.IsActive,
            DisplayOrder = currency.DisplayOrder,
            RateManagementMode = currency.RateManagementMode,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };
    }

    private static CurrencyListDto MapToListDto(Currency currency)
    {
        return new CurrencyListDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            DecimalPlaces = currency.DecimalPlaces,
            IsDefault = currency.IsDefault,
            IsActive = currency.IsActive,
            DisplayOrder = currency.DisplayOrder,
            RateManagementMode = currency.RateManagementMode,
            CreatedAt = currency.CreatedAt
        };
    }
}
