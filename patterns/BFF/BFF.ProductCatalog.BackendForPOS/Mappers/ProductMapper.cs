using BFF.ProductCatalog.Search;

namespace BFF.ProductCatalog.BackendForPOS.Mappers;
public class ProductMapper
{
    public static Models.Product Map(ProductIndexDocument document)
    {
        // For POS, we treat each variant as a separate product
        // Here we just map the first variant for simplicity
        var variant = document.Variants.FirstOrDefault();
        if (variant == null) return null;
        return new Models.Product
        {
            Id = variant.VariantId,
            Name = document.Name,
            Sku = variant.Sku,
            Price = variant.Price,
            Description = document.Description,
            Category = document.Category,
            StockQuantity = variant.InStock ? 100 : 0, // Simplified stock logic
            CreatedAt = variant.CreatedAt,
            UpdatedAt = variant.UpdatedAt,
            ImageUrl = document.ImageUrls.FirstOrDefault() ?? string.Empty,
            IsActive = variant.IsActive,
            Brand = document.Brand,
            Dimensions = variant.Dimensions.Select(d => new Models.Dimension
            {
                DimensionId = d.DimensionId,
                Name = d.Name,
                Value = d.Value,
                DisplayType = "text" // Simplified display type
            }).ToList()
        };
    }
}
