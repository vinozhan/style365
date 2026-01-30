using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Style365.Application.Common.Interfaces;
using Style365.Application.Features.Products.Commands.BulkImportProducts;

namespace Style365.Infrastructure.CsvParsing;

public class CsvParserService : ICsvParserService
{
    public async Task<List<ProductCsvRecord>> ParseProductCsvAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<ProductCsvRecordMap>();

        var records = new List<ProductCsvRecord>();
        await foreach (var record in csv.GetRecordsAsync<ProductCsvRecord>(cancellationToken))
        {
            records.Add(record);
        }

        return records;
    }
}

internal sealed class ProductCsvRecordMap : ClassMap<ProductCsvRecord>
{
    public ProductCsvRecordMap()
    {
        Map(m => m.Name).Name("Name", "ProductName", "name");
        Map(m => m.SKU).Name("SKU", "Sku", "sku");
        Map(m => m.Description).Name("Description", "description").Optional();
        Map(m => m.ShortDescription).Name("ShortDescription", "Short Description", "shortDescription").Optional();
        Map(m => m.Price).Name("Price", "price");
        Map(m => m.Currency).Name("Currency", "currency").Default("USD");
        Map(m => m.ComparePrice).Name("ComparePrice", "Compare Price", "comparePrice").Optional();
        Map(m => m.CostPrice).Name("CostPrice", "Cost Price", "costPrice").Optional();
        Map(m => m.StockQuantity).Name("StockQuantity", "Stock Quantity", "stockQuantity", "Stock").Default(0);
        Map(m => m.LowStockThreshold).Name("LowStockThreshold", "Low Stock Threshold", "lowStockThreshold").Default(5);
        Map(m => m.IsActive).Name("IsActive", "Is Active", "isActive", "Active").Default(true);
        Map(m => m.IsFeatured).Name("IsFeatured", "Is Featured", "isFeatured", "Featured").Default(false);
        Map(m => m.Weight).Name("Weight", "weight").Default(0);
        Map(m => m.WeightUnit).Name("WeightUnit", "Weight Unit", "weightUnit").Default("kg");
        Map(m => m.Brand).Name("Brand", "brand").Optional();
        Map(m => m.MetaTitle).Name("MetaTitle", "Meta Title", "metaTitle").Optional();
        Map(m => m.MetaDescription).Name("MetaDescription", "Meta Description", "metaDescription").Optional();
        Map(m => m.CategoryName).Name("CategoryName", "Category Name", "categoryName", "Category");
        Map(m => m.Tags).Name("Tags", "tags").Optional();
        Map(m => m.Size).Name("Size", "size", "Sizes").Optional();
        Map(m => m.Color).Name("Color", "color", "Colors").Optional();
        Map(m => m.ImageUrls).Name("ImageUrls", "Image URLs", "imageUrls", "Images").Optional();
    }
}
