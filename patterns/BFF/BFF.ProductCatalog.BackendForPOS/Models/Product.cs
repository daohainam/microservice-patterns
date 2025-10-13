namespace BFF.ProductCatalog.BackendForPOS.Models;

// in the POS system, we use a simplified product model, a product is actually a variant in the catalog system
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Brand { get; set; } = string.Empty;
    public List<Dimension> Dimensions { get; set; } = [];
}

public class Dimension
{
    public string DimensionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DisplayType { get; set; } = string.Empty;
}
