using B2BCommerce.Backend.Application.DTOs.Orders;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        // Shipping address
        RuleFor(x => x.ShippingStreet).NotEmpty().WithMessage("Shipping street is required");
        RuleFor(x => x.ShippingCity).NotEmpty().WithMessage("Shipping city is required");
        RuleFor(x => x.ShippingState).NotEmpty().WithMessage("Shipping state is required");
        RuleFor(x => x.ShippingCountry).NotEmpty().WithMessage("Shipping country is required");
        RuleFor(x => x.ShippingPostalCode).NotEmpty().WithMessage("Shipping postal code is required");

        RuleFor(x => x.OrderItems)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.OrderItems).SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue)
            .WithMessage("Discount amount cannot be negative");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).When(x => x.ShippingCost.HasValue)
            .WithMessage("Shipping cost cannot be negative");
    }
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue)
            .WithMessage("Discount amount cannot be negative");
    }
}
