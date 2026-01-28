using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Models;
using Style365.Application.Features.Products.Commands.CreateProduct;
using Style365.Application.Features.Products.Commands.UpdateProduct;
using Style365.Application.Features.Products.Commands.DeleteProduct;
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
    /// Retrieve products with advanced filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering, sorting, and pagination</param>
    /// <returns>Paginated list of products with metadata</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        // Validate query parameters
        if (query.Page < 1) query = query with { Page = 1 };
        if (query.PageSize < 1) query = query with { PageSize = 20 };
        if (query.PageSize > 100) query = query with { PageSize = 100 };
        
        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
        {
            return BadRequest(new { error = "Minimum price cannot be greater than maximum price" });
        }

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
    /// Update an existing product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Updated product details</param>
    /// <returns>Updated product information</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid product data</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { errors = new[] { "Route ID does not match command ID" } });
        }

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
    /// Delete a product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand(id);
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

    /// <summary>
    /// Get available product brands for filtering
    /// </summary>
    /// <returns>List of available brands</returns>
    /// <response code="200">Brands retrieved successfully</response>
    [HttpGet("brands")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<IActionResult> GetBrands()
    {
        // This would be implemented as a separate query, but for now using the existing structure
        var query = new GetProductsQuery { Page = 1, PageSize = 1000 };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        var brands = result.Data!.Items
            .Select(p => p.Brand)
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .OrderBy(b => b)
            .ToList();

        return Ok(brands);
    }

    /// <summary>
    /// Get price range for products (min/max prices)
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <returns>Price range information</returns>
    /// <response code="200">Price range retrieved successfully</response>
    [HttpGet("price-range")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPriceRange([FromQuery] Guid? categoryId = null)
    {
        var query = new GetProductsQuery 
        { 
            CategoryId = categoryId,
            Page = 1, 
            PageSize = 1000 
        };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        var products = result.Data!.Items.ToList();
        if (!products.Any())
        {
            return Ok(new { MinPrice = 0m, MaxPrice = 0m });
        }

        var priceRange = new
        {
            MinPrice = products.Min(p => p.Price),
            MaxPrice = products.Max(p => p.Price)
        };

        return Ok(priceRange);
    }

    /// <summary>
    /// Get search suggestions based on partial input
    /// </summary>
    /// <param name="query">Partial search term</param>
    /// <param name="limit">Maximum number of suggestions (default: 10)</param>
    /// <returns>List of search suggestions</returns>
    /// <response code="200">Suggestions retrieved successfully</response>
    [HttpGet("suggestions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<IActionResult> GetSearchSuggestions(
        [FromQuery] string query, 
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            return Ok(Array.Empty<string>());
        }

        var searchQuery = new GetProductsQuery 
        { 
            SearchTerm = query,
            Page = 1, 
            PageSize = Math.Min(limit * 2, 50) 
        };
        var result = await _mediator.Send(searchQuery);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        var suggestions = result.Data!.Items
            .Select(p => p.Name)
            .Where(name => name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .Take(limit)
            .ToList();

        return Ok(suggestions);
    }
}