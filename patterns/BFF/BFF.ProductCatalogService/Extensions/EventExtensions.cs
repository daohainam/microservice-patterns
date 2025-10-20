using BFF.ProductCatalog.Events;

namespace BFF.ProductCatalogService.Extensions;
public static class EventExtensions
{
    public static ProductCreatedEvent ToProductCreatedEvent(this Product product, ProductCatalogDbContext dbContext)
    {
        var dimensions = dbContext.Dimensions.Include(d => d.Values)
            .Where(d => product.Dimensions.Select(pd => pd.DimensionId).Contains(d.Id))
            .ToList();
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
                    UpdatedAt = v.UpdatedAt,
                    IsActive = v.IsActive,
                    DimensionValues = v.DimensionValues?.Select(dv => new VariantDimensionValueInfo()
                    {
                        DimensionId = dv.DimensionId,
                        Value = dv.Value
                    }).ToList() ?? []
                }).ToList() ?? [],
                Dimensions = [.. dimensions.Select(d => new DimensionInfo()
                { 
                    DimensionId = d.Id,
                    Name = d.Name,
                    DisplayType = d.DisplayType,
                    Values = d.Values?.Select(v => new DimensionValueInfo
                    {
                        Value = v.Value,
                        DisplayValue = v.DisplayValue ?? ""
                    }).ToList() ?? []
                }
                )],
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