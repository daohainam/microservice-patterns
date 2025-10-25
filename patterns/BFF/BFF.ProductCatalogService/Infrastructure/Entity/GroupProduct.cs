using System.Text.Json.Serialization;

namespace BFF.ProductCatalogService.Infrastructure.Entity;
[PrimaryKey(nameof(ProductId), nameof(GroupId))]
public class GroupProduct
{
    public Guid ProductId { get; set; }
    [JsonIgnore]
    public Product Product { get; set; } = default!;
    public Guid GroupId { get; set; }
    [JsonIgnore]
    public Group Group { get; set; } = default!;
}
