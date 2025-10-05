namespace BFF.ProductCatalogService.Infrastructure.Entity;
[PrimaryKey(nameof(ProductId), nameof(GroupId))]
public class ProductGroup
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = default!;
}
