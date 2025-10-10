using BFF.ProductCatalog.Events;
using System.Globalization;
using System.Text;

namespace BFF.ProductCatalog.Search;

public static class ProductEsMapper
{
    public static ProductIndexDocument Map(ProductCreatedEvent e)
    {
        var p = e.Product;

        // Dimension metadata lookup
        var dimById = new Dictionary<string, DimensionInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in p.Dimensions)
            dimById[d.DimensionId] = d;

        var variants = new List<VariantDoc>(p.Variants.Count);
        foreach (var v in p.Variants)
        {
            var dimsNested = new List<VariantDimensionDoc>(v.DimensionValues.Count);
            var dimsFlat = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var dv in v.DimensionValues)
            {
                // resolve name & display_value from metadata
                string dimId = dv.DimensionId;
                string name = dimById.TryGetValue(dimId, out var meta) ? meta.Name : dimId;
                string display = dv.Value;

                if (meta != null && meta.Values is { Count: > 0 })
                {
                    var match = meta.Values.Find(x => string.Equals(x.Value, dv.Value, StringComparison.OrdinalIgnoreCase));
                    if (match != null && !string.IsNullOrWhiteSpace(match.DisplayValue))
                        display = match.DisplayValue;
                }

                dimsNested.Add(new VariantDimensionDoc
                {
                    DimensionId = dimId,
                    Name = name,
                    Value = NormalizeKeyword(dv.Value),
                    DisplayValue = display
                });

                var key = NormalizeKeyword(name);
                var val = NormalizeKeyword(dv.Value);
                if (!dimsFlat.ContainsKey(key)) dimsFlat[key] = val;
            }

            variants.Add(new VariantDoc
            {
                VariantId = v.Id,
                Sku = v.Sku,
                BarCode = v.BarCode,
                Price = v.Price,
                InStock = v.InStock,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                Description = v.Description ?? "",
                Dimensions = dimsNested,
                DimsFlat = dimsFlat
            });
        }

        // Rollups
        decimal? priceMin = variants.Count > 0 ? variants.Min(x => x.Price) : null;
        var inStockVariants = variants.Where(x => x.InStock).ToList();
        decimal? priceMinInStock = inStockVariants.Count > 0 ? inStockVariants.Min(x => x.Price) : priceMin;
        bool hasStock = inStockVariants.Count > 0;
        var primary = ChoosePrimary(variants);

        // Category leaf + breadcrumb from Product.Path
        Guid categoryId = Guid.Empty;
        string categoryName = "";
        string categorySlug = "";
        string categoryPath = "";
        if (p.Path is { Count: > 0 })
        {
            var leaf = p.Path[^1];
            categoryId = leaf.CategoryId;
            categoryName = leaf.Name;
            categorySlug = leaf.UrlSlug;
            categoryPath = string.Join("/", p.Path.Select(x => x.UrlSlug));
        }

        return new ProductIndexDocument
        {
            ProductId = e.ProductId,

            Name = p.Name,
            Slug = p.UrlSlug,
            Description = p.Description ?? "",

            BrandId = p.Brand.BrandId,
            BrandName = p.Brand.Name,

            CategoryId = categoryId,
            CategoryName = categoryName,
            CategorySlug = categorySlug,
            CategoryPath = categoryPath,

            GroupIds = [.. p.Groups.Select(g => g.GroupId)],
            GroupNames = [.. p.Groups.Select(g => g.Name)],

            Images = [.. p.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new ProductImageDoc { Url = i.ImageUrl, Alt = i.AltText ?? "", SortOrder = i.SortOrder })],

            PriceMin = priceMin,
            PriceMinInStock = priceMinInStock,
            HasStock = hasStock,
            VariantCount = variants.Count,

            Variants = variants,
            PrimaryVariant = primary,

            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,

            Suggest = new SimpleCompletion { Input = [p.Name, p.Brand.Name] }
        };
    }

    private static PrimaryVariantDoc? ChoosePrimary(List<VariantDoc> variants)
    {
        if (variants.Count == 0) return null;
        var best = variants.Where(v => v.InStock).OrderBy(v => v.Price).FirstOrDefault()
               ?? variants.OrderBy(v => v.Price).First();
        return new PrimaryVariantDoc { VariantId = best.VariantId, Price = best.Price, InStock = best.InStock };
    }

    private static string NormalizeKeyword(string s)
    {
        s ??= "";
        var lower = s.Trim().ToLowerInvariant();
        return RemoveDiacritics(lower);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}