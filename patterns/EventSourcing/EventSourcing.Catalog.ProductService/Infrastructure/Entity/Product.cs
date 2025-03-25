namespace EventSourcing.Catalog.ProductService.Infrastructure.Entity;
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<Variant> Variants { get; set; } = [];
}
