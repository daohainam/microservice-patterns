namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class ProductDimention
{
    public Guid ProductId { get; set; }
    public string DimensionId { get; set; } = default!;
    public string Value { get; set; } = default!;
}
