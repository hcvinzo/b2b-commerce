using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Newsletter;

namespace B2BCommerce.Backend.Application.Features.Newsletter.Commands.SubscribeNewsletter;

/// <summary>
/// Command to subscribe to the newsletter
/// </summary>
public record SubscribeNewsletterCommand(string Email) : ICommand<Result<NewsletterSubscriptionDto>>;
