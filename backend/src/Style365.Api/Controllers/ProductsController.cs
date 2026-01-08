using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Common.DTOs;
using Style365.Application.Features.Products.Commands.CreateProduct;
using Style365.Application.Features.Products.Queries.GetProducts;
using Style365.Application.Features.Products.Queries.GetProductById;

namespace Style365.Api.Controllers;

/// <summary>
/// Product catalog management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieve products with filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering, sorting, and pagination</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    /// <param name="command">Product details including name, description, price, and category</param>
    /// <returns>Created product information</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetProducts), null, result.Data);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Products in category</returns>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProductsQuery
        {
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get featured products
    /// </summary>
    /// <param name="limit">Maximum number of products</param>
    /// <returns>Featured products</returns>
    /// <response code="200">Featured products retrieved</response>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 10)
    {
        var query = new GetProductsQuery
        {
            FeaturedOnly = true,
            PageSize = limit,
            Page = 1
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Search products
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Search results</returns>
    /// <response code="200">Search completed</response>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProductsQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }
}