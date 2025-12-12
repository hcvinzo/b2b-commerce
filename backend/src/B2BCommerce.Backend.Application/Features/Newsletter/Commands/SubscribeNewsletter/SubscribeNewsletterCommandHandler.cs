using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Newsletter;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Newsletter.Commands.SubscribeNewsletter;

/// <summary>
/// Handler for SubscribeNewsletterCommand
/// </summary>
public class SubscribeNewsletterCommandHandler : ICommandHandler<SubscribeNewsletterCommand, Result<NewsletterSubscriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeNewsletterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NewsletterSubscriptionDto>> Handle(SubscribeNewsletterCommand request, CancellationToken cancellationToken)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<NewsletterSubscriptionDto>.Failure("E-posta adresi gereklidir", "EMAIL_REQUIRED");
        }

        // Check if email is already subscribed
        var existingSubscription = await _unitOfWork.NewsletterSubscriptions.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (existingSubscription != null)
        {
            // If previously unsubscribed, resubscribe
            if (existingSubscription.UnsubscribedAt != null)
            {
                existingSubscription.Resubscribe();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<NewsletterSubscriptionDto>.Success(new NewsletterSubscriptionDto
                {
                    Id = existingSubscription.Id,
                    Email = existingSubscription.Email.Value,
                    SubscribedAt = existingSubscription.SubscribedAt,
                    IsVerified = existingSubscription.IsVerified,
                    Message = "Tekrar abone oldunuz! Lansman haberlerini e-posta adresinize göndereceğiz."
                });
            }

            // Already subscribed
            return Result<NewsletterSubscriptionDto>.Success(new NewsletterSubscriptionDto
            {
                Id = existingSubscription.Id,
                Email = existingSubscription.Email.Value,
                SubscribedAt = existingSubscription.SubscribedAt,
                IsVerified = existingSubscription.IsVerified,
                Message = "Bu e-posta adresi zaten kayıtlı. Lansman haberlerini paylaşacağız."
            });
        }

        // Create new subscription
        try
        {
            var subscription = NewsletterSubscription.Create(request.Email);

            await _unitOfWork.NewsletterSubscriptions.AddAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<NewsletterSubscriptionDto>.Success(new NewsletterSubscriptionDto
            {
                Id = subscription.Id,
                Email = subscription.Email.Value,
                SubscribedAt = subscription.SubscribedAt,
                IsVerified = subscription.IsVerified,
                Message = "Başarıyla abone oldunuz! Lansman haberlerini e-posta adresinize göndereceğiz."
            });
        }
        catch (ArgumentException ex)
        {
            return Result<NewsletterSubscriptionDto>.Failure(ex.Message, "INVALID_EMAIL");
        }
    }
}
