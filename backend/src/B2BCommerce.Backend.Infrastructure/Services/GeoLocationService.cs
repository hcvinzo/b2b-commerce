using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// GeoLocation service implementation
/// </summary>
public class GeoLocationService : IGeoLocationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(IUnitOfWork unitOfWork, ILogger<GeoLocationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<GeoLocationListDto>>> GetAllAsync(
        string? search,
        Guid? typeId,
        Guid? parentId,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(1, pageNumber);

        var query = _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .Include(l => l.Parent)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(lowerSearch) || l.Code.ToLower().Contains(lowerSearch));
        }

        if (typeId.HasValue)
        {
            query = query.Where(l => l.GeoLocationTypeId == typeId.Value);
        }

        if (parentId.HasValue)
        {
            query = query.Where(l => l.ParentId == parentId.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "code" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(l => l.Code)
                : query.OrderBy(l => l.Code),
            "type" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(l => l.Type.DisplayOrder).ThenByDescending(l => l.Type.Name)
                : query.OrderBy(l => l.Type.DisplayOrder).ThenBy(l => l.Type.Name),
            "depth" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(l => l.Depth)
                : query.OrderBy(l => l.Depth),
            _ => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(l => l.Name)
                : query.OrderBy(l => l.Name)
        };

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new GeoLocationListDto
            {
                Id = l.Id,
                GeoLocationTypeId = l.GeoLocationTypeId,
                GeoLocationTypeName = l.Type.Name,
                Code = l.Code,
                Name = l.Name,
                ParentId = l.ParentId,
                ParentName = l.Parent != null ? l.Parent.Name : null,
                PathName = l.PathName,
                Depth = l.Depth,
                ChildCount = l.Children.Count,
                ExternalCode = l.ExternalCode,
                ExternalId = l.ExternalId
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<GeoLocationListDto>(items, totalCount, pageNumber, pageSize);
        return Result<PagedResult<GeoLocationListDto>>.Success(result);
    }

    public async Task<Result<GeoLocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .Include(l => l.Parent)
            .Where(l => l.Id == id)
            .Select(l => new GeoLocationDto
            {
                Id = l.Id,
                GeoLocationTypeId = l.GeoLocationTypeId,
                GeoLocationTypeName = l.Type.Name,
                Code = l.Code,
                Name = l.Name,
                ParentId = l.ParentId,
                ParentName = l.Parent != null ? l.Parent.Name : null,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                Path = l.Path,
                PathName = l.PathName,
                Depth = l.Depth,
                Metadata = l.Metadata,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                ExternalCode = l.ExternalCode,
                ExternalId = l.ExternalId,
                LastSyncedAt = l.LastSyncedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            return Result<GeoLocationDto>.Failure("Geo location not found", "GEO_LOCATION_NOT_FOUND");
        }

        return Result<GeoLocationDto>.Success(location);
    }

    public async Task<Result<List<GeoLocationTreeDto>>> GetTreeAsync(Guid? typeId = null, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .AsQueryable();

        if (typeId.HasValue)
        {
            query = query.Where(l => l.GeoLocationTypeId == typeId.Value);
        }

        var allLocations = await query
            .OrderBy(l => l.Type.DisplayOrder)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

        // Build tree starting from root locations (ParentId is null)
        var rootLocations = allLocations.Where(l => l.ParentId is null).ToList();
        var tree = rootLocations.Select(l => BuildTreeNode(l, allLocations)).ToList();

        return Result<List<GeoLocationTreeDto>>.Success(tree);
    }

    public async Task<Result<List<GeoLocationListDto>>> GetByTypeAsync(Guid typeId, CancellationToken cancellationToken = default)
    {
        // Check if type exists
        var typeExists = await _unitOfWork.GeoLocationTypes.AnyAsync(t => t.Id == typeId, cancellationToken);
        if (!typeExists)
        {
            return Result<List<GeoLocationListDto>>.Failure("Geo location type not found", "GEO_LOCATION_TYPE_NOT_FOUND");
        }

        var locations = await _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .Include(l => l.Parent)
            .Where(l => l.GeoLocationTypeId == typeId)
            .OrderBy(l => l.Name)
            .Select(l => new GeoLocationListDto
            {
                Id = l.Id,
                GeoLocationTypeId = l.GeoLocationTypeId,
                GeoLocationTypeName = l.Type.Name,
                Code = l.Code,
                Name = l.Name,
                ParentId = l.ParentId,
                ParentName = l.Parent != null ? l.Parent.Name : null,
                PathName = l.PathName,
                Depth = l.Depth,
                ChildCount = l.Children.Count,
                ExternalCode = l.ExternalCode,
                ExternalId = l.ExternalId
            })
            .ToListAsync(cancellationToken);

        return Result<List<GeoLocationListDto>>.Success(locations);
    }

    public async Task<Result<List<GeoLocationListDto>>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        // If ParentId is provided, verify it exists
        if (parentId.HasValue)
        {
            var parentExists = await _unitOfWork.GeoLocations.AnyAsync(l => l.Id == parentId.Value, cancellationToken);
            if (!parentExists)
            {
                return Result<List<GeoLocationListDto>>.Failure("Parent geo location not found", "PARENT_NOT_FOUND");
            }
        }

        var locations = await _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .Include(l => l.Parent)
            .Where(l => l.ParentId == parentId)
            .OrderBy(l => l.Type.DisplayOrder)
            .ThenBy(l => l.Name)
            .Select(l => new GeoLocationListDto
            {
                Id = l.Id,
                GeoLocationTypeId = l.GeoLocationTypeId,
                GeoLocationTypeName = l.Type.Name,
                Code = l.Code,
                Name = l.Name,
                ParentId = l.ParentId,
                ParentName = l.Parent != null ? l.Parent.Name : null,
                PathName = l.PathName,
                Depth = l.Depth,
                ChildCount = l.Children.Count,
                ExternalCode = l.ExternalCode,
                ExternalId = l.ExternalId
            })
            .ToListAsync(cancellationToken);

        return Result<List<GeoLocationListDto>>.Success(locations);
    }

    public async Task<Result<GeoLocationDto>> CreateAsync(
        Guid geoLocationTypeId,
        string code,
        string name,
        Guid? parentId,
        decimal? latitude,
        decimal? longitude,
        string? metadata,
        CancellationToken cancellationToken = default)
    {
        // Validate geo location type exists
        var geoLocationType = await _unitOfWork.GeoLocationTypes.GetByIdAsync(geoLocationTypeId, cancellationToken);
        if (geoLocationType is null)
        {
            return Result<GeoLocationDto>.Failure("Geo location type not found", "GEO_LOCATION_TYPE_NOT_FOUND");
        }

        // Validate parent if provided
        GeoLocation? parent = null;
        string? parentPath = null;
        string? parentPathName = null;
        int parentDepth = -1;

        if (parentId.HasValue)
        {
            parent = await _unitOfWork.GeoLocations.GetByIdAsync(parentId.Value, cancellationToken);
            if (parent is null)
            {
                return Result<GeoLocationDto>.Failure("Parent geo location not found", "PARENT_NOT_FOUND");
            }
            parentPath = parent.Path;
            parentPathName = parent.PathName;
            parentDepth = parent.Depth;
        }

        // Check for duplicate code within the same type
        var existingWithCode = await _unitOfWork.GeoLocations.Query()
            .AnyAsync(l => l.GeoLocationTypeId == geoLocationTypeId && l.Code == code, cancellationToken);
        if (existingWithCode)
        {
            return Result<GeoLocationDto>.Failure("A geo location with this code already exists for this type", "DUPLICATE_CODE");
        }

        var geoLocation = GeoLocation.Create(
            geoLocationTypeId,
            code,
            name,
            parentId,
            latitude,
            longitude,
            metadata: metadata);

        // Set hierarchy info
        geoLocation.SetParent(parentId, parentPath, parentPathName, parentDepth);

        await _unitOfWork.GeoLocations.AddAsync(geoLocation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocation {Id} created: {Name} ({Code})", geoLocation.Id, name, code);

        return Result<GeoLocationDto>.Success(new GeoLocationDto
        {
            Id = geoLocation.Id,
            GeoLocationTypeId = geoLocation.GeoLocationTypeId,
            GeoLocationTypeName = geoLocationType.Name,
            Code = geoLocation.Code,
            Name = geoLocation.Name,
            ParentId = geoLocation.ParentId,
            ParentName = parent?.Name,
            Latitude = geoLocation.Latitude,
            Longitude = geoLocation.Longitude,
            Path = geoLocation.Path,
            PathName = geoLocation.PathName,
            Depth = geoLocation.Depth,
            Metadata = geoLocation.Metadata,
            CreatedAt = geoLocation.CreatedAt,
            UpdatedAt = geoLocation.UpdatedAt,
            ExternalCode = geoLocation.ExternalCode,
            ExternalId = geoLocation.ExternalId,
            LastSyncedAt = geoLocation.LastSyncedAt
        });
    }

    public async Task<Result<GeoLocationDto>> UpdateAsync(
        Guid id,
        string code,
        string name,
        decimal? latitude,
        decimal? longitude,
        string? metadata,
        CancellationToken cancellationToken = default)
    {
        var geoLocation = await _unitOfWork.GeoLocations.Query()
            .Include(l => l.Type)
            .Include(l => l.Parent)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (geoLocation is null)
        {
            return Result<GeoLocationDto>.Failure("Geo location not found", "GEO_LOCATION_NOT_FOUND");
        }

        // Check for duplicate code within the same type (excluding current)
        var existingWithCode = await _unitOfWork.GeoLocations.Query()
            .AnyAsync(l => l.GeoLocationTypeId == geoLocation.GeoLocationTypeId &&
                          l.Code == code &&
                          l.Id != id, cancellationToken);
        if (existingWithCode)
        {
            return Result<GeoLocationDto>.Failure("A geo location with this code already exists for this type", "DUPLICATE_CODE");
        }

        geoLocation.Update(code, name, latitude, longitude, metadata);

        // Update PathName if name changed
        if (geoLocation.PathName is not null && !geoLocation.PathName.EndsWith(name))
        {
            var parentPathName = geoLocation.Parent?.PathName;
            var newPathName = string.IsNullOrEmpty(parentPathName) ? name : $"{parentPathName}/{name}";
            geoLocation.UpdateHierarchy(geoLocation.Path, newPathName, geoLocation.Depth);
        }

        _unitOfWork.GeoLocations.Update(geoLocation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocation {Id} updated: {Name} ({Code})", id, name, code);

        return Result<GeoLocationDto>.Success(new GeoLocationDto
        {
            Id = geoLocation.Id,
            GeoLocationTypeId = geoLocation.GeoLocationTypeId,
            GeoLocationTypeName = geoLocation.Type?.Name ?? string.Empty,
            Code = geoLocation.Code,
            Name = geoLocation.Name,
            ParentId = geoLocation.ParentId,
            ParentName = geoLocation.Parent?.Name,
            Latitude = geoLocation.Latitude,
            Longitude = geoLocation.Longitude,
            Path = geoLocation.Path,
            PathName = geoLocation.PathName,
            Depth = geoLocation.Depth,
            Metadata = geoLocation.Metadata,
            CreatedAt = geoLocation.CreatedAt,
            UpdatedAt = geoLocation.UpdatedAt,
            ExternalCode = geoLocation.ExternalCode,
            ExternalId = geoLocation.ExternalId,
            LastSyncedAt = geoLocation.LastSyncedAt
        });
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var geoLocation = await _unitOfWork.GeoLocations.GetByIdAsync(id, cancellationToken);

        if (geoLocation is null)
        {
            return Result.Failure("Geo location not found", "GEO_LOCATION_NOT_FOUND");
        }

        // Check if there are child locations
        var hasChildren = await _unitOfWork.GeoLocations.Query()
            .AnyAsync(l => l.ParentId == id, cancellationToken);

        if (hasChildren)
        {
            return Result.Failure("Cannot delete geo location with child locations", "HAS_CHILDREN");
        }

        await _unitOfWork.GeoLocations.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GeoLocation {Id} deleted", id);

        return Result.Success();
    }

    private static GeoLocationTreeDto BuildTreeNode(GeoLocation location, List<GeoLocation> allLocations)
    {
        var children = allLocations
            .Where(l => l.ParentId == location.Id)
            .Select(l => BuildTreeNode(l, allLocations))
            .ToList();

        return new GeoLocationTreeDto
        {
            Id = location.Id,
            GeoLocationTypeId = location.GeoLocationTypeId,
            GeoLocationTypeName = location.Type?.Name ?? string.Empty,
            Code = location.Code,
            Name = location.Name,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Depth = location.Depth,
            Children = children,
            ExternalCode = location.ExternalCode,
            ExternalId = location.ExternalId
        };
    }
}
