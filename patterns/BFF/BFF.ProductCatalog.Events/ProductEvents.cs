using EventBus.Events;

namespace BFF.ProductCatalog.Events;

public class ProductCreatedEvent: IntegrationEvent
{
    public Guid ProductId { get; set; }
    public ProductInfo Product { get; set; } = default!;
    public CategoryInfo Category { get; set; } = default!;
    public BrandInfo Brand { get; set; } = default!;
    public List<VariantInfo> Variants { get; set; } = [];
    public List<DimensionInfo> Dimensions { get; set; } = [];
    public List<GroupInfo> Groups { get; set; } = [];
    public List<ProductImageInfo> Images { get; set; } = [];
}

public class ProductUpdatedEvent: IntegrationEvent
{
    public Guid ProductId { get; set; }
    public ProductInfo Product { get; set; } = default!;
}


public class ProductDeletedEvent: IntegrationEvent
{
    public Guid ProductId { get; set; }
}

public class ProductInfo
{
    public string Name { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid BrandId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; } = default!;
}

public class BrandInfo
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string LogoUrl { get; set; } = default!;
}

public class VariantInfo
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string BarCode { get; set; } = default!;
    public decimal Price { get; set; }
    public string Description { get; set; } = default!;
    public bool InStock { get; set; } // on front end we just need to know if it's in stock or not
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public List<DimensionValue> DimensionValues { get; set; } = [];

}

public class DimensionInfo
{
    public string DimensionId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DisplayType { get; set; } = default!; // "dropdown", "color", "text", "image", "choice"
    public List<string> Values { get; set; } = [];
}

public class CategoryInfo
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
}

public class GroupInfo
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = default!;
}

public class ProductImageInfo
{
    public Guid ImageId { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public string AltText { get; set; } = default!;
    public int SortOrder { get; set; }
}

public class DimensionValue
{
    public Guid DimensionId { get; set; }
    public string Value { get; set; } = default!;
    public string DisplayValue { get; set; } = default!;
}