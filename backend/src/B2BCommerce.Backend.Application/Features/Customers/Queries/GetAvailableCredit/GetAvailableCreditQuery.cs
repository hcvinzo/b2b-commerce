using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetAvailableCredit;

/// <summary>
/// Query to get customer's available credit
/// </summary>
public record GetAvailableCreditQuery(Guid CustomerId) : IQuery<Result<decimal>>;
