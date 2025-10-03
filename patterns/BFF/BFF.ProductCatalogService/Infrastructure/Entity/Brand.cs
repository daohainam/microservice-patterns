namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Brand
{
    public Guid Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}
