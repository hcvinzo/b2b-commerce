using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// GeoLocationType service implementation
/// </summary>
public class GeoLocationTypeService : IGeoLocationTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GeoLocationTypeService> _logger;

    public GeoLocationTypeService(IUnitOfWork unitOfWork, ILogger<GeoLocationTypeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<GeoLocationTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var types = await _unitOfWork.GeoLocationTypes.Query()
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .Select(t => new GeoLocationTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                DisplayOrder = t.DisplayOrder,
                LocationCount = t.Locations.Count,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<GeoLocationTypeDto>>.Success(types);
    }

    public async Task<Result<GeoLocationTypeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var type = await _unitOfWork.GeoLocationTypes.Query()
            .Where(t => t.Id == id)
            .Select(t => new GeoLocationTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                DisplayOrder = t.DisplayOrder,
                LocationCount = t.Locations.Count,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (type is null)
        {
            return Result<GeoLocationTypeDto>.Failure("Geo location type not found", "GEO_LOCATION_TYPE_NOT_FOUND");
        }

        return Result<GeoLocationTypeDto>.Success(type);
    }

    public async Task<Result<GeoLocationTypeDto>> CreateAsync(string name, int displayOrder, CancellationToken cancellationToken = default)
    {
        // Check if name already exists
        if (await _unitOfWork.GeoLocationTypes.ExistsByNameAsync(name, cancellationToken))
        {
            return Result<GeoLocationTypeDto>.Failure("A geo location type with this name already exists", "DUPLICATE_NAME");
        }

        var geoLocationType = GeoLocationType.Create(name, displayOrder);

        await _unitOfWork.GeoLocationTypes.AddAsync(geoLocationType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocationType {Id} created: {Name}", geoLocationType.Id, name);

        return Result<GeoLocationTypeDto>.Success(new GeoLocationTypeDto
        {
            Id = geoLocationType.Id,
            Name = geoLocationType.Name,
            DisplayOrder = geoLocationType.DisplayOrder,
            LocationCount = 0,
            CreatedAt = geoLocationType.CreatedAt,
            UpdatedAt = geoLocationType.UpdatedAt
        });
    }

    public async Task<Result<GeoLocationTypeDto>> UpdateAsync(Guid id, string name, int displayOrder, CancellationToken cancellationToken = default)
    {
        var geoLocationType = await _unitOfWork.GeoLocationTypes.GetByIdAsync(id, cancellationToken);

        if (geoLocationType is null)
        {
            return Result<GeoLocationTypeDto>.Failure("Geo location type not found", "GEO_LOCATION_TYPE_NOT_FOUND");
        }

        // Check if new name already exists (for different entity)
        var existingWithName = await _unitOfWork.GeoLocationTypes.GetByNameAsync(name, cancellationToken);
        if (existingWithName is not null && existingWithName.Id != id)
        {
            return Result<GeoLocationTypeDto>.Failure("A geo location type with this name already exists", "DUPLICATE_NAME");
        }

        geoLocationType.Update(name, displayOrder);
        _unitOfWork.GeoLocationTypes.Update(geoLocationType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocationType {Id} updated: {Name}", id, name);

        var locationCount = await _unitOfWork.GeoLocations.Query()
            .CountAsync(l => l.GeoLocationTypeId == id, cancellationToken);

        return Result<GeoLocationTypeDto>.Success(new GeoLocationTypeDto
        {
            Id = geoLocationType.Id,
            Name = geoLocationType.Name,
            DisplayOrder = geoLocationType.DisplayOrder,
            LocationCount = locationCount,
            CreatedAt = geoLocationType.CreatedAt,
            UpdatedAt = geoLocationType.UpdatedAt
        });
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var geoLocationType = await _unitOfWork.GeoLocationTypes.GetByIdAsync(id, cancellationToken);

        if (geoLocationType is null)
        {
            return Result.Failure("Geo location type not found", "GEO_LOCATION_TYPE_NOT_FOUND");
        }

        // Check if there are locations using this type
        var hasLocations = await _unitOfWork.GeoLocations.Query()
            .AnyAsync(l => l.GeoLocationTypeId == id, cancellationToken);

        if (hasLocations)
        {
            return Result.Failure("Cannot delete geo location type with existing locations", "HAS_LOCATIONS");
        }

        await _unitOfWork.GeoLocationTypes.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocationType {Id} deleted", id);

        return Result.Success();
    }
}
