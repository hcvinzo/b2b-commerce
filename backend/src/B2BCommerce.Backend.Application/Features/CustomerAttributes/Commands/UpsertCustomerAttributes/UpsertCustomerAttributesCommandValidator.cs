using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.UpsertCustomerAttributes;

/// <summary>
/// Validator for UpsertCustomerAttributesCommand
/// </summary>
public class UpsertCustomerAttributesCommandValidator : AbstractValidator<UpsertCustomerAttributesCommand>
{
    public UpsertCustomerAttributesCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.AttributeType)
            .IsInEnum()
            .WithMessage("Invalid attribute type");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items cannot be null");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.JsonData)
                    .NotEmpty()
                    .WithMessage("JSON data is required for each item");

                item.RuleFor(i => i.DisplayOrder)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Display order must be greater than or equal to 0");
            });
    }
}
