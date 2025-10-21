using BFF.ProductCatalog.Events;

namespace BFF.ProductCatalogService.Extensions;
public static class EventExtensions
{
    public async static Task<ProductCreatedEvent> ToProductCreatedEvent(this Product product, ApiServicesWithUnitOfWork services)
    {
        var brand = await services.UnitOfWork.DbContext.Brands.Where(b => b.Id == product.BrandId).SingleOrDefaultAsync(services.CancellationToken) ?? throw new Exception("Brand not found");
        var category = await services.UnitOfWork.DbContext.Categories.Where(c => c.Id == product.CategoryId).SingleOrDefaultAsync(services.CancellationToken) ?? throw new Exception("Category not found");

        var dimensionIds = product.Dimensions.Select(d => d.DimensionId).ToList();
        var dimensions = await services.UnitOfWork.DbContext.Dimensions.Where(d => dimensionIds.Contains(d.Id))
            .Include(d => d.Values)
            .ToListAsync(services.CancellationToken);

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
                        CategoryId = category.Id,
                        Name = category.Name ?? string.Empty,
                        Description = category.Description ?? string.Empty
                    }
                ],
                Brand = new BrandInfo
                {
                    BrandId = brand.Id,
                    Name = brand.Name ?? string.Empty,
                    Description = brand.Description ?? string.Empty,
                    LogoUrl = brand.LogoUrl ?? string.Empty
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
                Dimensions = dimensions.Select(d => new DimensionInfo
                {
                    DimensionId = d.Id,
                    Name = d.Name,
                    DisplayType = d.DisplayType,
                    Values = [.. d.Values.Select(v => new DimensionValueInfo
                    {
                        Value = v.Value,
                        DisplayValue = v.DisplayValue ?? string.Empty
                    })]
                }).ToList() ?? [],
                Groups = product.Groups.Select(g => new GroupInfo
                {
                    GroupId = g.Id,
                    Name = g.Name
                }).ToList() ?? [],
                Images = product.Images.Select(i => new ProductImageInfo
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