using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Categories.Commands.CreateCategory;
using Style365.Application.Features.Categories.Commands.DeleteCategory;
using Style365.Application.Features.Categories.Commands.UpdateCategory;
using Style365.Application.Features.Categories.Commands.UploadCategoryImage;
using Style365.Application.Features.Categories.Queries.GetCategories;
using Style365.Application.Features.Categories.Queries.GetCategoryById;

namespace Style365.Api.Controllers;

/// <summary>
/// Category management endpoints for organizing products
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all categories with optional filtering
    /// </summary>
    /// <param name="activeOnly">Filter to active categories only</param>
    /// <param name="includeSubCategories">Include subcategories in response</param>
    /// <param name="parentId">Filter by parent category ID</param>
    /// <returns>List of categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetCategoriesResponse), 200)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool? activeOnly = true,
        [FromQuery] bool includeSubCategories = false,
        [FromQuery] Guid? parentId = null)
    {
        var query = new GetCategoriesQuery
        {
            ActiveOnly = activeOnly,
            IncludeSubCategories = includeSubCategories,
            ParentId = parentId
        };

        var result = await _mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="includeSubCategories">Include subcategories in response</param>
    /// <param name="includeProducts">Include products in response</param>
    /// <returns>Category details</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(
        Guid id,
        [FromQuery] bool includeSubCategories = false,
        [FromQuery] bool includeProducts = false)
    {
        var query = new GetCategoryByIdQuery
        {
            Id = id,
            IncludeSubCategories = includeSubCategories,
            IncludeProducts = includeProducts
        };

        var result = await _mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="command">Category details</param>
    /// <returns>Created category</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CreateCategoryResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(
            nameof(GetCategory), 
            new { id = result.Data!.Id }, 
            result.Data);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Updated category details</param>
    /// <returns>No content</returns>
    /// <response code="204">Category updated successfully</response>
    /// <response code="400">Invalid input or circular reference</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Category not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="force">Force delete even if category has products</param>
    /// <returns>No content</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="400">Category has subcategories or products</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Category not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCategory(Guid id, [FromQuery] bool force = false)
    {
        var command = new DeleteCategoryCommand
        {
            Id = id,
            Force = force
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Get category tree structure
    /// </summary>
    /// <returns>Hierarchical category structure</returns>
    /// <response code="200">Category tree retrieved successfully</response>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(GetCategoriesResponse), 200)]
    public async Task<IActionResult> GetCategoryTree()
    {
        var query = new GetCategoriesQuery
        {
            ActiveOnly = true,
            IncludeSubCategories = true,
            ParentId = null
        };

        var result = await _mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Upload an image for a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="file">Image file to upload</param>
    /// <returns>Upload result with image URL</returns>
    /// <response code="200">Image uploaded successfully</response>
    /// <response code="400">Invalid request or validation errors</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Category not found</response>
    [HttpPost("{id}/image")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UploadCategoryImageResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { errors = new[] { "No file provided" } });
        }

        var command = new UploadCategoryImageCommand
        {
            CategoryId = id,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length
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
}