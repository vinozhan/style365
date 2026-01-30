using Style365.Application.Features.Products.Commands.BulkImportProducts;

namespace Style365.Application.Common.Interfaces;

public interface ICsvParserService
{
    Task<List<ProductCsvRecord>> ParseProductCsvAsync(Stream stream, CancellationToken cancellationToken = default);
}
