using System.Text.Json.Serialization;

namespace BFF.ProductCatalog.Search;
public sealed class ProductIndexDocument
{
    [JsonPropertyName("product_id")] public Guid ProductId { get; init; }

    [JsonPropertyName("name")] public string Name { get; set; } = default!;
    [JsonPropertyName("slug")] public string Slug { get; set; } = default!;
    [JsonPropertyName("description")] public string Description { get; set; } = "";

    [JsonPropertyName("brand_id")] public Guid BrandId { get; set; }
    [JsonPropertyName("brand_name")] public string BrandName { get; set; } = default!;

    [JsonPropertyName("category_id")] public Guid CategoryId { get; set; }
    [JsonPropertyName("category_name")] public string CategoryName { get; set; } = default!;
    [JsonPropertyName("category_slug")] public string CategorySlug { get; set; } = default!;
    [JsonPropertyName("category_path")] public string CategoryPath { get; set; } = default!; // "root/child/leaf"

    [JsonPropertyName("group_ids")] public List<Guid> GroupIds { get; set; } = [];
    [JsonPropertyName("group_names")] public List<string> GroupNames { get; set; } = [];

    [JsonPropertyName("images")] public List<ImageDoc> Images { get; set; } = [];

    // Rollups
    [JsonPropertyName("price_min")] public decimal? PriceMin { get; set; }
    [JsonPropertyName("in_stock")] public bool InStock { get; set; }
    [JsonPropertyName("variant_count")] public int VariantCount { get; set; }

    [JsonPropertyName("dimensions")] public List<DimensionDoc> Dimensions { get; init; } = [];

    [JsonPropertyName("variants")] public List<VariantDoc> Variants { get; set; } = [];

    [JsonPropertyName("primary_variant")] public PrimaryVariantDoc? PrimaryVariant { get; set; }

    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }

    // Completion suggester
    [JsonPropertyName("suggest")] public SimpleCompletion? Suggest { get; set; }
}

public sealed class SimpleCompletion
{
    [JsonPropertyName("input")] public string[] Input { get; set; } = [];
}

public sealed class DimensionDoc
{
    [JsonPropertyName("dimension_id")] public string DimensionId { get; init; } = default!;
    [JsonPropertyName("name")] public string Name { get; init; } = default!;
    [JsonPropertyName("display_type")] public string DisplayType { get; init; } = default!;
}

public sealed class ImageDoc
{
    [JsonPropertyName("url")] public string Url { get; init; } = default!;
    [JsonPropertyName("alt")] public string Alt { get; init; } = "";
    [JsonPropertyName("sort_order")] public int SortOrder { get; init; }
}

public sealed class VariantDoc
{
    [JsonPropertyName("variant_id")] public Guid VariantId { get; init; }
    [JsonPropertyName("sku")] public string Sku { get; init; } = default!;
    [JsonPropertyName("barcode")] public string BarCode { get; init; } = default!;
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("in_stock")] public bool InStock { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("description")] public string Description { get; init; } = "";

    [JsonPropertyName("dimensions")] public List<VariantDimensionDoc> Dimensions { get; init; } = [];
    [JsonPropertyName("dims_flat")] public Dictionary<string, string> DimsFlat { get; init; } = new(StringComparer.Ordinal);

    [JsonPropertyName("images")] public List<ImageDoc> Images { get; set; } = [];
}

public sealed class VariantDimensionDoc
{
    [JsonPropertyName("dimension_id")] public string DimensionId { get; init; } = default!;
    [JsonPropertyName("value")] public string Value { get; init; } = default!;
    [JsonPropertyName("display_value")] public string DisplayValue { get; init; } = default!;
}

public sealed class PrimaryVariantDoc
{
    [JsonPropertyName("variant_id")] public Guid VariantId { get; init; }
    [JsonPropertyName("price")] public decimal Price { get; init; }
    [JsonPropertyName("in_stock")] public bool InStock { get; init; }
}
