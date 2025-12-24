using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.CreateCustomerDocument;
using B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.DeleteCustomerDocument;
using B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.UpdateCustomerDocument;
using B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocument;
using B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

/// <summary>
/// Controller for managing customer documents
/// </summary>
[ApiController]
[Route("api/customers/{customerId:guid}/documents")]
[Authorize]
public class CustomerDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all documents for a customer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerDocumentsQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid customerId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerDocumentQuery(customerId, id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create/upload a new document for a customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid customerId,
        [FromBody] CreateCustomerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerDocumentCommand(
            customerId,
            dto.DocumentType,
            dto.FileName,
            dto.FileType,
            dto.ContentUrl,
            dto.FileSize);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { customerId, id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Create a document for a customer during registration (anonymous access)
    /// </summary>
    [HttpPost("registration")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CustomerDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForRegistration(
        Guid customerId,
        [FromBody] CreateCustomerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerDocumentCommand(
            customerId,
            dto.DocumentType,
            dto.FileName,
            dto.FileType,
            dto.ContentUrl,
            dto.FileSize);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { customerId, id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Update/replace an existing document
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid customerId,
        Guid id,
        [FromBody] UpdateCustomerDocumentDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerDocumentCommand(
            customerId,
            id,
            dto.FileName,
            dto.FileType,
            dto.ContentUrl,
            dto.FileSize);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "DOCUMENT_NOT_FOUND")
            {
                return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            return BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid customerId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerDocumentCommand(customerId, id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
        }

        return NoContent();
    }
}
