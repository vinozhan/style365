using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Products.Commands.DeleteProductImage;
using Style365.Application.Features.Products.Commands.SetPrimaryProductImage;
using Style365.Application.Features.Products.Commands.UploadProductImages;

namespace Style365.Api.Controllers;

/// <summary>
/// Product image management endpoints
/// </summary>
[ApiController]
[Route("api/products/{productId}/images")]
[Authorize(Policy = "AdminOnly")]
public class ProductImagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload images for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="files">Image files to upload</param>
    /// <param name="altText">Optional alt text for all uploaded images</param>
    /// <returns>Upload results including URLs for each uploaded image</returns>
    /// <response code="200">Images uploaded successfully</response>
    /// <response code="400">Invalid request or validation errors</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Product not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(UploadProductImagesResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [RequestSizeLimit(100_000_000)] // 100MB limit for multiple files
    public async Task<IActionResult> UploadImages(
        Guid productId,
        [FromForm] IFormFileCollection files,
        [FromForm] string? altText = null)
    {
        var fileDtos = files.Select(f => new FileUploadDto
        {
            Stream = f.OpenReadStream(),
            FileName = f.FileName,
            ContentType = f.ContentType,
            Length = f.Length
        }).ToList();

        var command = new UploadProductImagesCommand
        {
            ProductId = productId,
            Files = fileDtos,
            AltText = altText
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a product image
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Image deleted successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Product or image not found</response>
    [HttpDelete("{imageId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId)
    {
        var command = new DeleteProductImageCommand
        {
            ProductId = productId,
            ImageId = imageId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Set an image as the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Primary image set successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Product or image not found</response>
    [HttpPut("{imageId}/set-primary")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetPrimaryImage(Guid productId, Guid imageId)
    {
        var command = new SetPrimaryProductImageCommand
        {
            ProductId = productId,
            ImageId = imageId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }
}
