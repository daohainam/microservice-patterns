namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } 
    public List<Variant> Variants { get; set; } = [];
    public List<Dimension> Dimensions { get; set; } = [];
}
