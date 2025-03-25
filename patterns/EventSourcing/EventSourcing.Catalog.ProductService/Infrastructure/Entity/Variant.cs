namespace EventSourcing.Catalog.ProductService.Infrastructure.Entity;
public class Variant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public Guid ProductId { get; set; }
    public string Size { get; set; } = default!;
    public string Color { get; set; } = default!;
}
