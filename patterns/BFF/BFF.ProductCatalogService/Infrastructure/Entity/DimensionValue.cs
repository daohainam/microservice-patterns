namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class DimensionValue
{
    public Guid Id { get; set; }
    public string DimensionId { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? DisplayValue { get; set; }
    public int SortOrder { get; set; }
}
