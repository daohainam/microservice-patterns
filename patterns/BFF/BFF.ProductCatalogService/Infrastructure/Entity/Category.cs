namespace BFF.ProductCatalogService.Infrastructure.Entity;
public class Category
{
    //public readonly static Guid ROOT_ID = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
    public Guid Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string UrlSlug { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
}
