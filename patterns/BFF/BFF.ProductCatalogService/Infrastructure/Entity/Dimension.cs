namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Dimension
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DisplayType { get; set; } = default!; // "dropdown", "color", "text", "image", "choice"
    public string DefaultValue { get; set; } = default!;
    public List<DimensionValue> Values { get; set; } = [];
}
