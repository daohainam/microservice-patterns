namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Image
{
    public Guid Id { get; set; }
    public string BaseUrl { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string AltText { get; set; } = default!;
}
