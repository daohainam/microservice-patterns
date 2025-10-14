using BFF.ProductCatalog.Search;

namespace BFF.ProductCatalog.BackendForPOS.Mappers;
public class ProductMapper
{
    public static IEnumerable<Models.Product> Map(ProductIndexDocument document)
    {
        // For POS, we treat each variant as a separate product
        
        var products = new List<Models.Product>();

        foreach (var variant in document.Variants)
        {
            var image = variant.Images.FirstOrDefault() ?? document.Images.FirstOrDefault();

            products.Add(new Models.Product()
            {
                Id = variant.VariantId,
                Name = document.Name,
                Sku = variant.Sku,
                Price = variant.Price,
                Description = document.Description,
                CategoryId = document.CategoryId,
                CategoryName = document.CategoryName,
                InStock = variant.InStock, 
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt,
                ImageUrl = image?.Url ?? "",
                IsActive = variant.IsActive,
                BrandId = document.BrandId,
                BrandName = document.BrandName,
                Dimensions = [.. variant.Dimensions.Select(d => new Models.Dimension
                {
                    DimensionId = d.DimensionId,
                    Value = d.Value,
                    DisplayType = document.Dimensions.FirstOrDefault(dim => dim.DimensionId == d.DimensionId)?.DisplayType ?? "text"
                })]
            });
        }

        return products;
    }
}
