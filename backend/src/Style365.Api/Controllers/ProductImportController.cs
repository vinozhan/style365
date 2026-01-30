using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Products.Commands.BulkImportProducts;

namespace Style365.Api.Controllers;

/// <summary>
/// Product bulk import endpoints
/// </summary>
[ApiController]
[Route("api/products/import")]
[Authorize(Policy = "AdminOnly")]
public class ProductImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Import products from a CSV file
    /// </summary>
    /// <param name="file">CSV file containing product data</param>
    /// <param name="validateOnly">If true, only validates the data without creating products</param>
    /// <param name="skipDuplicates">If true, skips rows with existing SKUs instead of reporting errors</param>
    /// <returns>Import results with success/error counts and details</returns>
    /// <response code="200">Import completed (check results for success/error details)</response>
    /// <response code="400">Invalid request or CSV format errors</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPost("csv")]
    [ProducesResponseType(typeof(BulkImportProductsResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> ImportFromCsv(
        IFormFile file,
        [FromQuery] bool validateOnly = false,
        [FromQuery] bool skipDuplicates = true)
    {
        var command = new BulkImportProductsCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ValidateOnly = validateOnly,
            SkipDuplicates = skipDuplicates
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Download a CSV template for product import
    /// </summary>
    /// <returns>CSV file with headers and sample data</returns>
    /// <response code="200">CSV template file</response>
    [HttpGet("template")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public IActionResult DownloadTemplate()
    {
        var csvContent = GenerateCsvTemplate();
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        return File(bytes, "text/csv", "product_import_template.csv");
    }

    private static string GenerateCsvTemplate()
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("Name,SKU,Description,ShortDescription,Price,Currency,ComparePrice,CostPrice,StockQuantity,LowStockThreshold,IsActive,IsFeatured,Weight,WeightUnit,Brand,MetaTitle,MetaDescription,CategoryName,Tags,Size,Color,ImageUrls");

        // Sample data rows
        sb.AppendLine("\"Classic Cotton T-Shirt\",\"TSH-001\",\"A comfortable everyday cotton t-shirt perfect for casual wear.\",\"Soft cotton t-shirt\",29.99,USD,39.99,15.00,100,10,true,true,0.2,kg,\"Style365\",\"Classic Cotton T-Shirt | Style365\",\"Shop our classic cotton t-shirt. Available in multiple sizes and colors.\",\"T-Shirts\",\"casual|cotton|summer\",\"S|M|L|XL\",\"Black|White|Navy\",\"https://example.com/images/tshirt1.jpg|https://example.com/images/tshirt2.jpg\"");

        sb.AppendLine("\"Slim Fit Jeans\",\"JNS-001\",\"Modern slim fit jeans with stretch comfort.\",\"Stylish slim jeans\",59.99,USD,79.99,30.00,50,5,true,false,0.5,kg,\"Style365\",\"Slim Fit Jeans | Style365\",\"Premium slim fit jeans for the modern look.\",\"Jeans\",,\"28|30|32|34|36\",\"Blue|Black\",");

        sb.AppendLine("\"Summer Dress\",\"DRS-001\",\"Light and airy summer dress for warm days.\",\"Breezy summer dress\",45.00,USD,,20.00,75,8,true,true,0.3,kg,\"Style365\",,,\"Dresses\",\"summer|casual|women\",,\"Red|Yellow|Green\",");

        return sb.ToString();
    }
}
