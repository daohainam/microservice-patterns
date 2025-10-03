namespace BFF.ProductCatalogService.Infrastructure.Entity;
internal sealed class DimensionDisplayTypes
{
    // "dropdown", "color", "text", "image", "choice"
    public const string Text = "text";
    public const string Color = "color";
    public const string Image = "image";
    public const string Dropdown = "dropdown";
    public const string Choice = "choice";

    public static readonly string[] All = [ Text, Color, Image, Dropdown, Choice ];

    public static bool Has(string displayType) => All.Contains(displayType);
}