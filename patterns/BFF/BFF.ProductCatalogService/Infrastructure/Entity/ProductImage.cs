namespace BFF.ProductCatalogService.Infrastructure.Entity;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
    public int SortOrder { get; set; }
}
