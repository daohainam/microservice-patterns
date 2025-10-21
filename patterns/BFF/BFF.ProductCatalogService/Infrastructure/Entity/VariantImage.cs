using System.Text.Json.Serialization;

namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class VariantImage
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public Guid ImageId { get; set; }
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    [JsonIgnore]
    public Image Image { get; set; } = default!;
}
