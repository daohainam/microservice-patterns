using System.Text.Json.Serialization;

namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid BrandId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CategoryId { get; set; } = default!;
    public List<Variant> Variants { get; set; } = [];
    public List<ProductDimension> Dimensions { get; set; } = [];
    public List<Group> Groups { get; set; } = [];
    public Brand Brand { get; set; } = default!;
}
