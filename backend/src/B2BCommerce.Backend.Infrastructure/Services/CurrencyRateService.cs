using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.CurrencyRates;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Currency rate service implementation
/// </summary>
public class CurrencyRateService : ICurrencyRateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CurrencyRateService> _logger;

    public CurrencyRateService(
        IUnitOfWork unitOfWork,
        ILogger<CurrencyRateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<CurrencyRateListDto>>> GetAllAsync(
        CurrencyRateFiltersDto? filters = null,
        CancellationToken cancellationToken = default)
    {
        var rates = await _unitOfWork.CurrencyRates.GetRatesAsync(
            filters?.FromCurrency,
            filters?.ToCurrency,
            filters?.ActiveOnly,
            cancellationToken);

        var dtos = rates.Select(MapToListDto).ToList();
        return Result<List<CurrencyRateListDto>>.Success(dtos);
    }

    public async Task<Result<CurrencyRateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rate = await _unitOfWork.CurrencyRates.GetByIdAsync(id, cancellationToken);

        if (rate is null)
        {
            return Result<CurrencyRateDto>.Failure($"Currency rate with ID {id} not found", "NOT_FOUND");
        }

        return Result<CurrencyRateDto>.Success(MapToDto(rate));
    }

    public async Task<Result<CurrencyRateDto>> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default)
    {
        var rate = await _unitOfWork.CurrencyRates.GetRateAsync(
            fromCurrency.ToUpperInvariant(),
            toCurrency.ToUpperInvariant(),
            null,
            cancellationToken);

        if (rate is null)
        {
            return Result<CurrencyRateDto>.Failure(
                $"Currency rate from {fromCurrency} to {toCurrency} not found", "NOT_FOUND");
        }

        return Result<CurrencyRateDto>.Success(MapToDto(rate));
    }

    public async Task<Result<CurrencyRateDto>> CreateAsync(CreateCurrencyRateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if rate already exists for this currency pair
            if (await _unitOfWork.CurrencyRates.ExistsAsync(dto.FromCurrency, dto.ToCurrency, null, cancellationToken))
            {
                return Result<CurrencyRateDto>.Failure(
                    $"Currency rate from {dto.FromCurrency} to {dto.ToCurrency} already exists", "DUPLICATE");
            }

            // Validate that currencies exist
            var fromCurrency = await _unitOfWork.Currencies.GetByCodeAsync(dto.FromCurrency.ToUpperInvariant(), cancellationToken);
            if (fromCurrency is null)
            {
                return Result<CurrencyRateDto>.Failure(
                    $"Source currency '{dto.FromCurrency}' not found", "VALIDATION");
            }

            var toCurrency = await _unitOfWork.Currencies.GetByCodeAsync(dto.ToCurrency.ToUpperInvariant(), cancellationToken);
            if (toCurrency is null)
            {
                return Result<CurrencyRateDto>.Failure(
                    $"Target currency '{dto.ToCurrency}' not found", "VALIDATION");
            }

            // Ensure effectiveDate is in UTC (PostgreSQL requires timestamptz)
            DateTime? effectiveDate = dto.EffectiveDate.HasValue
                ? DateTime.SpecifyKind(dto.EffectiveDate.Value, DateTimeKind.Utc)
                : null;

            var rate = CurrencyRate.Create(
                dto.FromCurrency,
                dto.ToCurrency,
                dto.Rate,
                effectiveDate);

            await _unitOfWork.CurrencyRates.AddAsync(rate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Currency rate created: {FromCurrency} -> {ToCurrency} = {Rate}",
                rate.FromCurrency, rate.ToCurrency, rate.Rate);

            return Result<CurrencyRateDto>.Success(MapToDto(rate));
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError(ex, "Error creating currency rate: {FromCurrency} -> {ToCurrency}. Inner: {InnerMessage}",
                dto.FromCurrency, dto.ToCurrency, innerMessage);
            return Result<CurrencyRateDto>.Failure($"Error creating currency rate: {innerMessage}");
        }
    }

    public async Task<Result<CurrencyRateDto>> UpdateAsync(Guid id, UpdateCurrencyRateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _unitOfWork.CurrencyRates.GetByIdForUpdateAsync(id, cancellationToken);

            if (rate is null)
            {
                return Result<CurrencyRateDto>.Failure($"Currency rate with ID {id} not found", "NOT_FOUND");
            }

            // Ensure effectiveDate is in UTC (PostgreSQL requires timestamptz)
            DateTime? effectiveDate = dto.EffectiveDate.HasValue
                ? DateTime.SpecifyKind(dto.EffectiveDate.Value, DateTimeKind.Utc)
                : null;

            rate.UpdateRate(dto.Rate, effectiveDate);

            _unitOfWork.CurrencyRates.Update(rate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Currency rate updated: {FromCurrency} -> {ToCurrency} = {Rate}",
                rate.FromCurrency, rate.ToCurrency, rate.Rate);

            return Result<CurrencyRateDto>.Success(MapToDto(rate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency rate: {CurrencyRateId}", id);
            return Result<CurrencyRateDto>.Failure($"Error updating currency rate: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _unitOfWork.CurrencyRates.GetByIdForUpdateAsync(id, cancellationToken);

            if (rate is null)
            {
                return Result.Failure($"Currency rate with ID {id} not found", "NOT_FOUND");
            }

            rate.MarkAsDeleted();
            _unitOfWork.CurrencyRates.Update(rate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Currency rate deleted: {FromCurrency} -> {ToCurrency}",
                rate.FromCurrency, rate.ToCurrency);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting currency rate: {CurrencyRateId}", id);
            return Result.Failure($"Error deleting currency rate: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyRateDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _unitOfWork.CurrencyRates.GetByIdForUpdateAsync(id, cancellationToken);

            if (rate is null)
            {
                return Result<CurrencyRateDto>.Failure($"Currency rate with ID {id} not found", "NOT_FOUND");
            }

            rate.Activate();
            _unitOfWork.CurrencyRates.Update(rate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Currency rate activated: {FromCurrency} -> {ToCurrency}",
                rate.FromCurrency, rate.ToCurrency);

            return Result<CurrencyRateDto>.Success(MapToDto(rate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating currency rate: {CurrencyRateId}", id);
            return Result<CurrencyRateDto>.Failure($"Error activating currency rate: {ex.Message}");
        }
    }

    public async Task<Result<CurrencyRateDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _unitOfWork.CurrencyRates.GetByIdForUpdateAsync(id, cancellationToken);

            if (rate is null)
            {
                return Result<CurrencyRateDto>.Failure($"Currency rate with ID {id} not found", "NOT_FOUND");
            }

            rate.Deactivate();
            _unitOfWork.CurrencyRates.Update(rate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Currency rate deactivated: {FromCurrency} -> {ToCurrency}",
                rate.FromCurrency, rate.ToCurrency);

            return Result<CurrencyRateDto>.Success(MapToDto(rate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating currency rate: {CurrencyRateId}", id);
            return Result<CurrencyRateDto>.Failure($"Error deactivating currency rate: {ex.Message}");
        }
    }

    private static CurrencyRateDto MapToDto(CurrencyRate rate)
    {
        return new CurrencyRateDto
        {
            Id = rate.Id,
            FromCurrency = rate.FromCurrency,
            ToCurrency = rate.ToCurrency,
            Rate = rate.Rate,
            EffectiveDate = rate.EffectiveDate,
            IsActive = rate.IsActive,
            CreatedAt = rate.CreatedAt,
            UpdatedAt = rate.UpdatedAt
        };
    }

    private static CurrencyRateListDto MapToListDto(CurrencyRate rate)
    {
        return new CurrencyRateListDto
        {
            Id = rate.Id,
            FromCurrency = rate.FromCurrency,
            ToCurrency = rate.ToCurrency,
            Rate = rate.Rate,
            EffectiveDate = rate.EffectiveDate,
            IsActive = rate.IsActive,
            UpdatedAt = rate.UpdatedAt
        };
    }
}
