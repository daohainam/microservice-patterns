namespace BFF.ProductCatalog.Events;
public class ProductUpdatedEvent
{
    public Guid ProductId { get; set; }
    public ProductInfo Product { get; set; } = default!;
    public List<VariantInfo> Variants { get; set; } = [];
    public List<DimensionInfo> Dimensions { get; set; } = [];
}

public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
    public ProductInfo Product { get; set; } = default!;
    public List<VariantInfo> Variants { get; set; } = [];
    public List<DimensionInfo> Dimensions { get; set; } = [];
}

public class ProductDeletedEvent
{
    public Guid ProductId { get; set; }
}

public class ProductInfo
{
    public string Name { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
}

public class VariantInfo
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string BarCode { get; set; } = default!;
    public decimal Price { get; set; }
    public string Description { get; set; } = default!;
    public bool InStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
}

public class DimensionInfo
{
    public string DimensionId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DisplayType { get; set; } = default!; // "dropdown", "color", "text", "image", "choice"
    public string DefaultValue { get; set; } = default!;
    public List<string> Values { get; set; } = [];
}
