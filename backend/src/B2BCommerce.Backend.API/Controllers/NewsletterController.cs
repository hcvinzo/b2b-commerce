using B2BCommerce.Backend.Application.DTOs.Newsletter;
using B2BCommerce.Backend.Application.Features.Newsletter.Commands.SubscribeNewsletter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewsletterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Subscribe to the newsletter
    /// </summary>
    /// <param name="request">Subscription request with email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription result</returns>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NewsletterSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterDto request, CancellationToken cancellationToken)
    {
        var command = new SubscribeNewsletterCommand(request.Email);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }
}
