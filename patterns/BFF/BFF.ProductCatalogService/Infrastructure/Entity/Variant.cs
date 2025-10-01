namespace BFF.ProductCatalogService.Infrastructure.Entity
{
    public class Variant
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; }
        public string Description { get; set; } = default!;
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<VariantDimensionValue> DimensionValues { get; set; } = [];
    }
}
