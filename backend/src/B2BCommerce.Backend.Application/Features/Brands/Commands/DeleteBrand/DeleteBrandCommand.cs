using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Brands.Commands.DeleteBrand;

/// <summary>
/// Command to delete a brand (soft delete)
/// </summary>
public record DeleteBrandCommand(Guid Id, string? DeletedBy) : ICommand<Result>;
