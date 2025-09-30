namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class DimensionValue
{
    public string Id { get; set; } = default!;
    public string DimensionId { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? DisplayValue { get; set; }
}
