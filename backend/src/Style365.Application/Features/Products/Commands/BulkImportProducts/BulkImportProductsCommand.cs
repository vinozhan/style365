using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.BulkImportProducts;

public record BulkImportProductsCommand : ICommand<Result<BulkImportProductsResult>>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public bool ValidateOnly { get; init; } = false;
    public bool SkipDuplicates { get; init; } = true;
}

public class BulkImportProductsResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public bool ValidateOnly { get; set; }
    public List<ImportedProductResult> ImportedProducts { get; set; } = [];
    public List<ImportRowError> Errors { get; set; } = [];
    public List<SkippedRowResult> SkippedRows { get; set; } = [];
}

public class ImportedProductResult
{
    public int RowNumber { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int VariantsCreated { get; set; }
}

public class ImportRowError
{
    public int RowNumber { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];
}

public class SkippedRowResult
{
    public int RowNumber { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
