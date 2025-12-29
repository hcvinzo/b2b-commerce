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

        RuleFor(x => x.AttributeDefinitionId)
            .NotEmpty()
            .WithMessage("Attribute definition ID is required");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items cannot be null");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Value)
                    .NotEmpty()
                    .WithMessage("Value is required for each item");
            });
    }
}
