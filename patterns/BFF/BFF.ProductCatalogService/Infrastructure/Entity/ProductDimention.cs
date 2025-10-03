using System.Text.Json.Serialization;

namespace BFF.ProductCatalogService.Infrastructure.Entity;

[PrimaryKey(nameof(ProductId), nameof(DimensionId))]
public class ProductDimention
{
    public Guid ProductId { get; set; }
    public string DimensionId { get; set; } = default!;
    public int SortOrder { get; set; }
    [JsonIgnore]
    public Product? Product { get; set; }
    [JsonIgnore]
    public Dimension? Dimension { get; set; }
}
