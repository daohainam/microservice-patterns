using BFF.ProductCatalog.Events;

namespace BFF.ProductCatalogService.Extensions;
public static class EventExtensions
{
    public static ProductCreatedEvent ToProductCreatedEvent(this Product product)
    {
        return new ProductCreatedEvent
        {
            ProductId = product.Id,
            Product = new ProductInfo
            {
                Name = product.Name,
                UrlSlug = product.UrlSlug,
                Description = product.Description,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,

                Path = [
                    new() {
                        CategoryId = product.Category?.Id ?? Guid.Empty,
                        Name = product.Category?.Name ?? string.Empty,
                        Description = product.Category?.Description ?? string.Empty
                    }
                ],
                Brand = new BrandInfo
                {
                    BrandId = product.Brand?.Id ?? Guid.Empty,
                    Name = product.Brand?.Name ?? string.Empty,
                    Description = product.Brand?.Description ?? string.Empty,
                    LogoUrl = product.Brand?.LogoUrl ?? string.Empty
                },
                Variants = product.Variants?.Select(v => new VariantInfo
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Sku = v.Sku,
                    BarCode = v.BarCode,
                    Price = v.Price,
                    Description = v.Description,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                }).ToList() ?? [],
                Dimensions = product.Dimensions?.OrderBy(d => d.SortOrder).Select(d => new DimensionInfo
                {
                    DimensionId = d.DimensionId,
                    Name = d.Dimension?.Name ?? string.Empty,
                    DisplayType = d.Dimension?.DisplayType ?? string.Empty,
                    Values = d.Dimension!.Values?.Select(v => new DimensionValueInfo() { 
                        DimensionId = v.DimensionId, 
                        Value = v.Value, 
                        DisplayValue = v.DisplayValue ?? ""
                    }).ToList() ?? []
                }).ToList() ?? [],
                Groups = product.Groups?.Select(g => new GroupInfo
                {
                    GroupId = g.Id,
                    Name = g.Name
                }).ToList() ?? [],
                Images = product.Images?.Select(i => new ProductImageInfo
                {
                    ImageId = i.Id,
                    AltText = i.AltText,
                    SortOrder = i.SortOrder,
                    ImageUrl = string.IsNullOrEmpty(i.Image?.BaseUrl) ? new Uri(new Uri(i.Image?.BaseUrl!), i.Image!.FileName).ToString() : i.Image.FileName
                }).ToList() ?? []
            },
        };
    }
}