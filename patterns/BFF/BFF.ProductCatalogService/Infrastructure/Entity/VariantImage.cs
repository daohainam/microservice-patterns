namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class VariantImage
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public Guid ImageId { get; set; }
    public int SortOrder { get; set; }
}
