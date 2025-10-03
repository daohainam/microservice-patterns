namespace BFF.ProductCatalogService.Apis;
public class Sortings
{
    public const string PriceAsc = "price-asc";
    public const string PriceDesc = "price-desc";
    public const string NameAsc = "name-asc";
    public const string NameDesc = "name-desc";
    public const string CreatedOnAsc = "createdon-asc";
    public const string CreatedOnDesc = "createdon-desc";
    public static readonly List<string> All =
    [
        PriceAsc, PriceDesc,
        NameAsc, NameDesc,
        CreatedOnAsc, CreatedOnDesc
    ];
}
