namespace BFF.ProductCatalogService.Infrastructure.Entity;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Image Image { get; set; } = default!;
}
