namespace BFF.ProductCatalogService.Infrastructure.Entity;
[PrimaryKey(nameof(VariantId), nameof(DimensionId))]
public class VariantDimensionValue
{
    public Guid VariantId { get; set; }
    public string DimensionId { get; set; } = default!;
    public string Value { get; set; } = default!;
}
