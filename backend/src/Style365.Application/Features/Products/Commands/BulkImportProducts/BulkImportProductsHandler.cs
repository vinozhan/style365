using Microsoft.Extensions.Logging;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.ValueObjects;

namespace Style365.Application.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsHandler : ICommandHandler<BulkImportProductsCommand, Result<BulkImportProductsResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICsvParserService _csvParserService;
    private readonly ILogger<BulkImportProductsHandler> _logger;

    public BulkImportProductsHandler(
        IUnitOfWork unitOfWork,
        ICsvParserService csvParserService,
        ILogger<BulkImportProductsHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _csvParserService = csvParserService;
        _logger = logger;
    }

    public async Task<Result<BulkImportProductsResult>> Handle(BulkImportProductsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = new BulkImportProductsResult
            {
                ValidateOnly = request.ValidateOnly
            };

            // Parse CSV file
            var records = await _csvParserService.ParseProductCsvAsync(request.FileStream, cancellationToken);
            result.TotalRows = records.Count;

            if (records.Count == 0)
            {
                return Result.Failure<BulkImportProductsResult>("CSV file is empty or has no valid records");
            }

            // Get all categories for lookup
            var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            var categoryLookup = categories.ToDictionary(c => c.Name.ToLowerInvariant(), c => c.Id);

            // Get all tags for lookup
            var tags = await _unitOfWork.ProductTags.GetAllAsync(cancellationToken);
            var tagLookup = tags.ToDictionary(t => t.Name.ToLowerInvariant(), t => t);

            // Get existing SKUs for duplicate check
            var existingProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            var existingSkus = new HashSet<string>(
                existingProducts.Select(p => p.Sku.ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase);

            var rowNumber = 1; // CSV header is row 0
            foreach (var record in records)
            {
                rowNumber++;
                var rowErrors = new List<string>();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(record.Name))
                    rowErrors.Add("Name is required");

                if (string.IsNullOrWhiteSpace(record.SKU))
                    rowErrors.Add("SKU is required");

                if (record.Price <= 0)
                    rowErrors.Add("Price must be greater than 0");

                if (string.IsNullOrWhiteSpace(record.CategoryName))
                    rowErrors.Add("Category name is required");

                // Check for duplicate SKU
                var sku = record.SKU?.Trim().ToUpperInvariant() ?? "";
                if (existingSkus.Contains(sku))
                {
                    if (request.SkipDuplicates)
                    {
                        result.SkippedRows.Add(new SkippedRowResult
                        {
                            RowNumber = rowNumber,
                            SKU = record.SKU ?? string.Empty,
                            Reason = "SKU already exists"
                        });
                        result.SkippedCount++;
                        continue;
                    }
                    else
                    {
                        rowErrors.Add($"SKU '{record.SKU}' already exists");
                    }
                }

                // Validate category exists
                Guid? categoryId = null;
                if (!string.IsNullOrWhiteSpace(record.CategoryName))
                {
                    var categoryKey = record.CategoryName.Trim().ToLowerInvariant();
                    if (categoryLookup.TryGetValue(categoryKey, out var catId))
                    {
                        categoryId = catId;
                    }
                    else
                    {
                        rowErrors.Add($"Category '{record.CategoryName}' not found");
                    }
                }

                if (rowErrors.Count > 0)
                {
                    result.Errors.Add(new ImportRowError
                    {
                        RowNumber = rowNumber,
                        SKU = record.SKU ?? string.Empty,
                        Name = record.Name ?? string.Empty,
                        Errors = rowErrors
                    });
                    result.ErrorCount++;
                    continue;
                }

                if (request.ValidateOnly)
                {
                    result.ImportedProducts.Add(new ImportedProductResult
                    {
                        RowNumber = rowNumber,
                        Name = record.Name ?? string.Empty,
                        SKU = record.SKU ?? string.Empty,
                        VariantsCreated = CountVariants(record)
                    });
                    result.SuccessCount++;
                    existingSkus.Add(sku); // Prevent duplicate SKUs in same batch
                    continue;
                }

                // Create the product
                try
                {
                    var product = await CreateProduct(record, categoryId!.Value, tagLookup, cancellationToken);
                    existingSkus.Add(sku);

                    result.ImportedProducts.Add(new ImportedProductResult
                    {
                        RowNumber = rowNumber,
                        ProductId = product.Id,
                        Name = product.Name,
                        SKU = product.Sku,
                        VariantsCreated = product.Variants.Count
                    });
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product for row {RowNumber}", rowNumber);
                    result.Errors.Add(new ImportRowError
                    {
                        RowNumber = rowNumber,
                        SKU = record.SKU ?? string.Empty,
                        Name = record.Name ?? string.Empty,
                        Errors = [$"Failed to create product: {ex.Message}"]
                    });
                    result.ErrorCount++;
                }
            }

            if (!request.ValidateOnly && result.SuccessCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Bulk import completed: {Total} rows, {Success} imported, {Skipped} skipped, {Errors} errors (ValidateOnly: {ValidateOnly})",
                result.TotalRows, result.SuccessCount, result.SkippedCount, result.ErrorCount, request.ValidateOnly);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk import");
            return Result.Failure<BulkImportProductsResult>($"Failed to process import: {ex.Message}");
        }
    }

    private async Task<Product> CreateProduct(
        ProductCsvRecord record,
        Guid categoryId,
        Dictionary<string, ProductTag> tagLookup,
        CancellationToken cancellationToken)
    {
        var price = Money.Create(record.Price, record.Currency);

        Money? comparePrice = null;
        if (record.ComparePrice.HasValue && record.ComparePrice.Value > 0)
        {
            comparePrice = Money.Create(record.ComparePrice.Value, record.Currency);
        }

        Money? costPrice = null;
        if (record.CostPrice.HasValue && record.CostPrice.Value >= 0)
        {
            costPrice = Money.Create(record.CostPrice.Value, record.Currency);
        }

        var product = new Product(record.Name.Trim(), record.SKU.Trim(), price, categoryId, record.Description?.Trim());

        product.UpdatePricing(price, comparePrice, costPrice);
        product.UpdateInventory(record.StockQuantity, record.LowStockThreshold, true);
        product.UpdatePhysicalProperties(record.Weight, record.WeightUnit);
        product.UpdateBasicInfo(record.Name.Trim(), record.Description?.Trim(), record.ShortDescription?.Trim(), record.Brand?.Trim());
        product.UpdateSeo(record.MetaTitle?.Trim(), record.MetaDescription?.Trim());
        product.SetFeatured(record.IsFeatured);

        if (!record.IsActive)
        {
            product.Deactivate();
        }

        // Add tags
        if (!string.IsNullOrWhiteSpace(record.Tags))
        {
            var tagNames = record.Tags.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tagName in tagNames)
            {
                var tagKey = tagName.ToLowerInvariant();
                if (tagLookup.TryGetValue(tagKey, out var tag))
                {
                    product.AddTag(tag);
                }
            }
        }

        // Create variants if sizes or colors are specified
        var sizes = ParsePipeDelimited(record.Size);
        var colors = ParsePipeDelimited(record.Color);

        if (sizes.Count > 0 || colors.Count > 0)
        {
            var variantCombinations = GenerateVariantCombinations(sizes, colors);
            var variantIndex = 1;

            foreach (var (size, color) in variantCombinations)
            {
                var variantName = GenerateVariantName(record.Name, size, color);
                var variantSku = $"{record.SKU}-{variantIndex:D3}";
                var variant = new ProductVariant(variantName, variantSku, price, record.StockQuantity);
                variant.UpdateAttributes(size, color, null, null);
                product.AddVariant(variant);
                variantIndex++;
            }
        }

        // Add image URLs if provided
        if (!string.IsNullOrWhiteSpace(record.ImageUrls))
        {
            var urls = ParsePipeDelimited(record.ImageUrls);
            var isFirst = true;
            var sortOrder = 0;

            foreach (var url in urls)
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    var image = new ProductImage(url, null, sortOrder, isFirst);
                    product.AddImage(image);
                    isFirst = false;
                    sortOrder++;
                }
            }
        }

        await _unitOfWork.Products.AddAsync(product, cancellationToken);

        return product;
    }

    private static List<string> ParsePipeDelimited(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static List<(string? Size, string? Color)> GenerateVariantCombinations(List<string> sizes, List<string> colors)
    {
        var combinations = new List<(string?, string?)>();

        if (sizes.Count == 0 && colors.Count == 0)
            return combinations;

        if (sizes.Count == 0)
        {
            combinations.AddRange(colors.Select(c => ((string?)null, (string?)c)));
        }
        else if (colors.Count == 0)
        {
            combinations.AddRange(sizes.Select(s => ((string?)s, (string?)null)));
        }
        else
        {
            foreach (var size in sizes)
            {
                combinations.AddRange(colors.Select(color => ((string?)size, (string?)color)));
            }
        }

        return combinations;
    }

    private static string GenerateVariantName(string productName, string? size, string? color)
    {
        var parts = new List<string> { productName };
        if (!string.IsNullOrEmpty(size)) parts.Add(size);
        if (!string.IsNullOrEmpty(color)) parts.Add(color);
        return string.Join(" - ", parts);
    }

    private static int CountVariants(ProductCsvRecord record)
    {
        var sizes = ParsePipeDelimited(record.Size);
        var colors = ParsePipeDelimited(record.Color);

        if (sizes.Count == 0 && colors.Count == 0)
            return 0;

        if (sizes.Count == 0)
            return colors.Count;

        if (colors.Count == 0)
            return sizes.Count;

        return sizes.Count * colors.Count;
    }
}
