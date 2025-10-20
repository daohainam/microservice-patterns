using BFF.ProductCatalogService.Extensions;
using TransactionalOutbox.Abstractions;

namespace BFF.ProductCatalogService.Apis;

public static class ProductCatalogApi
{
    private static readonly string InvalidDisplayType = $"Invalid display type. Valid types are: {string.Join(", ", DimensionDisplayTypes.All)}";
    private const int defaultPageSize = 10;

    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/bff/v1")
              .MapCatalogApi()
              .WithTags("Product Catalog Api");

        return builder;
    }

    public static RouteGroupBuilder MapCatalogApi(this RouteGroupBuilder group)
    {
        var categoryApiGroup = group.MapGroup("categories").WithTags("Category");
        categoryApiGroup.MapPost("/", CreateCategory);

        var brandApiGroup = group.MapGroup("brands").WithTags("Brand");
        brandApiGroup.MapPost("/", CreateBrand);
        brandApiGroup.MapGet("/", FindBrands);
        brandApiGroup.MapGet("/{brandId:guid}", FindBrandById);
        brandApiGroup.MapPut("/{brandId:guid}", UpdateBrand);

        var groupApiGroup = group.MapGroup("groups").WithTags("Group");
        groupApiGroup.MapPost("/", CreateGroup);

        var dimensionApiGroup = group.MapGroup("dimensions").WithTags("Dimension");
        dimensionApiGroup.MapPost("/", CreateDimentions);
        dimensionApiGroup.MapPost("/{id}/values", AddDimentionValues);
        dimensionApiGroup.MapGet("/", async ([AsParameters] ApiServices services, [FromQuery] int? offset = 0, [FromQuery] int? limit = defaultPageSize) =>
        {
            return await services.DbContext.Dimensions
            .Include(d => d.Values)
            .Skip(offset!.Value).Take(limit!.Value)
            .ToListAsync();
        });

        #region Products and Variants

        var productApiGroup = group.MapGroup("products").WithTags("Product");

        productApiGroup.MapPost("/", CreateProduct);
        productApiGroup.MapPut("/{productId:guid}", UpdateProduct);
        productApiGroup.MapGet("/", async ([AsParameters] ApiServices services, [FromQuery] int? offset = 0, [FromQuery] int? limit = defaultPageSize) =>
        {
            return await services.DbContext.Products
            // .Where(p => !p.IsDeleted) // in this in internal API, we return all products except deleted ones
            .Skip(offset!.Value).Take(limit!.Value).ToListAsync();
        });
        productApiGroup.MapGet("/{productId:guid}", async ([AsParameters] ApiServices services, Guid productId) =>
        {
            return await services.DbContext.Products.Include(p => p.Variants).ThenInclude(d => d.DimensionValues).Where(p => p.Id == productId).SingleOrDefaultAsync();
        });
        productApiGroup.MapGet("/{productId:guid}/dimensions", async ([AsParameters] ApiServices services, Guid productId) =>
        {
            var dimensions = from pd in services.DbContext.ProductDimensions
                             join d in services.DbContext.Dimensions on pd.DimensionId equals d.Id
                             where pd.ProductId == productId
                             select d;

            return await dimensions.Include(d => d.Values).ToListAsync();
        });
        productApiGroup.MapPost("/{productId:guid}/dimensions", AddProductDimension);

        productApiGroup.MapGet("/{productId:guid}/variants", FindVariants);
        productApiGroup.MapPost("/{productId:guid}/variants", CreateVariant);
        productApiGroup.MapPut("/{productId:guid}/variants/{variantId:guid}", UpdateVariant);
        #endregion

        return group;
    }

    #region Brands
    private static async Task<Results<Ok<Brand>, BadRequest, BadRequest<string>>> UpdateBrand([AsParameters] ApiServices services, Guid brandId, Brand brand)
    {
        if (brand == null || brand.Id != brandId)
        {
            return TypedResults.BadRequest();
        }

        var existingBrand = await services.DbContext.Brands.FindAsync(brandId);
        if (existingBrand == null)
        {
            return TypedResults.BadRequest("Brand not found.");
        }
        
        if (string.IsNullOrEmpty(brand.Name))
        {
            return TypedResults.BadRequest("Category Name is required.");
        }
        
        if (string.IsNullOrEmpty(brand.UrlSlug))
        {
            return TypedResults.BadRequest("Category UrlSlug is required.");
        }
        
        existingBrand.Name = brand.Name;
        existingBrand.Description = brand.Description;
        existingBrand.UrlSlug = brand.UrlSlug;
        existingBrand.LogoUrl = brand.LogoUrl;

        services.DbContext.Brands.Update(existingBrand);
        await services.DbContext.SaveChangesAsync();
        
        return TypedResults.Ok(existingBrand);
    }

    private static async Task<Results<Ok<Brand>, NotFound>> FindBrandById([AsParameters] ApiServices services, Guid brandId)
    {
        var brand = await services.DbContext.Brands.FindAsync(brandId);
        if (brand == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(brand);
    }

    private static async Task<Results<Ok<Brand[]>, NotFound>> FindBrands([AsParameters] ApiServices services, [FromQuery] int? offset = 0, [FromQuery] int? limit = defaultPageSize)
    {
        var brands = await services.DbContext.Brands
            .Skip(offset!.Value).Take(limit!.Value)
            .ToArrayAsync();
        
        return TypedResults.Ok(brands);
    }

    private static async Task<Results<Ok<Brand>, BadRequest, BadRequest<string>>> CreateBrand([AsParameters] ApiServices services, Brand brand)
    {
        if (brand == null)
        {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrEmpty(brand.Name))
        {
            return TypedResults.BadRequest("Category Name is required.");
        }

        if (string.IsNullOrEmpty(brand.UrlSlug))
        {
            return TypedResults.BadRequest("Category UrlSlug is required.");
        }

        if (brand.Id == Guid.Empty)
            brand.Id = Guid.CreateVersion7();

        await services.DbContext.Brands.AddAsync(brand);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(brand);
    }
    #endregion

    #region Variants
    private static async Task<Results<Ok<Variant>, BadRequest, NotFound>> UpdateVariant([AsParameters] ApiServices services, Guid productId, Guid variantId, Variant variant)
    {
        if (variant == null || variant.Id != variantId)
        {
            return TypedResults.BadRequest();
        }
        var existingVariant = await services.DbContext.Variants.FindAsync(variantId);
        if (existingVariant == null || existingVariant.ProductId != productId)
        {
            return TypedResults.NotFound();
        }

        // do not update stock, it should be updated via inventory service only
        existingVariant.Sku = variant.Sku;
        existingVariant.BarCode = variant.BarCode;
        existingVariant.Price = variant.Price;
        existingVariant.Description = variant.Description;
        existingVariant.IsActive = variant.IsActive;
        existingVariant.IsDeleted = variant.IsDeleted;
        existingVariant.UpdatedAt = DateTime.UtcNow;

        services.DbContext.Variants.Update(existingVariant);
        await services.DbContext.SaveChangesAsync();
        return TypedResults.Ok(existingVariant);
    }

    private static async Task<Results<Ok<Variant[]>, BadRequest, BadRequest<string>, NotFound>> CreateVariant([AsParameters] ApiServices services, Guid productId, Variant[] variants)
    {
        if (variants == null || variants.Length == 0)
        {
            return TypedResults.BadRequest();
        }

        var product = await services.DbContext.Products.Where(p => p.Id == productId).SingleOrDefaultAsync();
        if (product == null)
        {
            return TypedResults.NotFound();
        }

        var dimensions = await services.DbContext.ProductDimensions.Where(pd => pd.ProductId == productId)
                                    .Include(d => d.Dimension).ThenInclude(dv => dv.Values)
                                    .Select(pd => pd.Dimension)
                                    .ToListAsync();

        foreach (var variant in variants)
        {
            if (variant.Id == Guid.Empty)
                variant.Id = Guid.CreateVersion7();
            variant.ProductId = productId;
            variant.CreatedAt = DateTime.UtcNow;
            variant.UpdatedAt = DateTime.UtcNow;

            variant.Description ??= string.Empty;
            variant.BarCode ??= string.Empty;

            // validate dimension values
            if (dimensions.Count != variant.DimensionValues!.Count)
            {
                return TypedResults.BadRequest("All product dimensions must be specified for the variant.");
            }

            foreach (var dim in dimensions)
            {
                var variantDimValue = variant.DimensionValues.FirstOrDefault(dv => dv.DimensionId == dim.Id);
                if (variantDimValue == null)
                {
                    return TypedResults.BadRequest($"Dimension '{dim.Id}' must be specified for the variant.");
                }
                if (!dim.Values.Any(v => v.Value == variantDimValue.Value))
                {
                    return TypedResults.BadRequest($"Invalid value '{variantDimValue.Value}' for dimension '{dim.Id}'.");
                }
            }

            await services.DbContext.Variants.AddAsync(variant);
            foreach (var dimValue in variant.DimensionValues)
            {
                dimValue.VariantId = variant.Id;
                await services.DbContext.VariantDimensionValues.AddAsync(dimValue);
            }
        }

        await services.DbContext.SaveChangesAsync();
        return TypedResults.Ok(variants);
    }

    private static async Task<Results<Ok<List<Variant>>, NotFound>> FindVariants([AsParameters] ApiServices services, Guid productId)
    {
        var product = await services.DbContext.Products.FindAsync(productId);
        if (product == null)
        {
            return TypedResults.NotFound();
        }

        var variants = await services.DbContext.Variants.Where(v => v.ProductId == productId).ToListAsync();
        return TypedResults.Ok(variants);
    }
    #endregion

    private static async Task<Results<Ok, BadRequest, BadRequest<string>>> AddProductDimension([AsParameters] ApiServices services, Guid productId, Dimension[] dimensions)
    {
        if (dimensions == null || dimensions.Length == 0)
        {
            return TypedResults.BadRequest();
        }

        if (productId == Guid.Empty)
            return TypedResults.BadRequest("Product Id is required.");

        foreach (var dimension in dimensions)
        {
            if (string.IsNullOrEmpty(dimension.Id))
                return TypedResults.BadRequest("Dimension Id is required.");

            var existingDimension = await services.DbContext.ProductDimensions.FindAsync(productId, dimension.Id);
            if (existingDimension == null)
            {
                await services.DbContext.ProductDimensions.AddAsync(new ProductDimension()
                {
                    ProductId = productId,
                    DimensionId = dimension.Id
                });
            }
        }

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<Group>, BadRequest, BadRequest<string>>> CreateGroup([AsParameters] ApiServices services, Group group)
    {
        if (group == null)
        {
            return TypedResults.BadRequest();
        }
        if (string.IsNullOrEmpty(group.Name))
        {
            return TypedResults.BadRequest("Group Name is required.");
        }

        if (group.Id == Guid.Empty)
            group.Id = Guid.CreateVersion7();

        await services.DbContext.Groups.AddAsync(group);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(group);
    }

    private static async Task<Results<Ok<Category>, BadRequest, BadRequest<string>>> CreateCategory([AsParameters] ApiServices services, Category category)
    {
        if (category == null)
        {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrEmpty(category.Name))
        {
            return TypedResults.BadRequest("Category Name is required.");
        }

        if (string.IsNullOrEmpty(category.UrlSlug))
        {
            return TypedResults.BadRequest("Category UrlSlug is required.");
        }

        if (category.Id == Guid.Empty)
            category.Id = Guid.CreateVersion7();

        await services.DbContext.Categories.AddAsync(category);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(category);
    }

    private static async Task<Results<Ok<DimensionValue[]>, BadRequest, BadRequest<string>>> AddDimentionValues([AsParameters] ApiServices services, [FromRoute] string id, DimensionValue[] dimensionValues)
    {
        if (dimensionValues == null || dimensionValues.Length == 0)
        {
            return TypedResults.BadRequest();
        }

        foreach (var dimensionValue in dimensionValues)
        {
            if (dimensionValue.Id == Guid.Empty)
                dimensionValue.Id = Guid.CreateVersion7();

            dimensionValue.DimensionId = id;

            await services.DbContext.DimensionValues.AddAsync(dimensionValue);
        }
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(dimensionValues);
    }

    private static async Task<Results<Ok<Dimension[]>, BadRequest, BadRequest<string>>> CreateDimentions([AsParameters] ApiServices services, Dimension[] dimensions)
    {
        if (dimensions == null || dimensions.Length == 0)
        {
            return TypedResults.BadRequest();
        }

        foreach (var dimension in dimensions)
        {
            if (string.IsNullOrEmpty(dimension.Id))
                return TypedResults.BadRequest("Dimension Id is required.");
            if (string.IsNullOrEmpty(dimension.DisplayType))
                dimension.DisplayType = DimensionDisplayTypes.Text;
            else if (!DimensionDisplayTypes.Has(dimension.DisplayType))
                return TypedResults.BadRequest(InvalidDisplayType);

            if (!IsValidDimensionId(dimension.Id))
            {
                return TypedResults.BadRequest("Dimension Id can only contain alphanumeric characters and underscores.");
            }

            dimension.DefaultValue ??= "";
            await services.DbContext.Dimensions.AddAsync(dimension);

            if (dimension.Values != null && dimension.Values.Count > 0)
            {
                foreach (var value in dimension.Values)
                {
                    if (value.Id == Guid.Empty)
                        value.Id = Guid.CreateVersion7();
                    value.DimensionId = dimension.Id;
                }
                await services.DbContext.DimensionValues.AddRangeAsync(dimension.Values);
            }
        }

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(dimensions);
    }

    private static bool IsValidDimensionId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (!(id[0] >= 'a' && id[0] <= 'z')) // must start with a lowercase letter
        {
            return false;
        }

        return id.All(c => c == '_' || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')); // only allow lowercase letters, numbers and underscores
    }

    private static async Task<Results<Ok<Product>, BadRequest, BadRequest<string>>> CreateProduct([AsParameters] ApiServicesWithUnitOfWork services, Product product)
    {
        if (product == null)
        {
            return TypedResults.BadRequest();
        }

        if (product.Id == Guid.Empty)
            product.Id = Guid.CreateVersion7();

        if (product.Groups != null && product.Groups.Count > 0)
        {
            // we do not allow creating new groups via product creation
            return TypedResults.BadRequest("Only existing groups can be associated with the product.");
        }

        await services.UnitOfWork.BeginTransactionAsync(services.CancellationToken);

        var brand = await services.UnitOfWork.DbContext.Brands.FindAsync(product.BrandId) ?? throw new Exception("Brand not found.");
        var category = await services.UnitOfWork.DbContext.Categories.FindAsync(product.CategoryId) ?? throw new Exception("Category not found.");
        var dimensions = await services.UnitOfWork.DbContext.ProductDimensions
                                    .ToListAsync();

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.UrlSlug ??= product.Id.ToString();

        if (product.Dimensions != null && product.Dimensions.Count > 0)
        {
            foreach (var dimension in product.Dimensions)
            {
                if (string.IsNullOrEmpty(dimension.DimensionId))
                    return TypedResults.BadRequest("Dimension Id is required.");
            }
        }

        if (product.Variants != null && product.Variants.Count > 0)
        {
            foreach (var variant in product.Variants)
            {
                if (variant.Id == Guid.Empty)
                    variant.Id = Guid.CreateVersion7();
                variant.ProductId = product.Id;
                variant.CreatedAt = DateTime.UtcNow;
                variant.UpdatedAt = DateTime.UtcNow;
                variant.Description ??= string.Empty;
                variant.BarCode ??= string.Empty;
                variant.Sku ??= string.Empty;

                await services.UnitOfWork.DbContext.Variants.AddAsync(variant);

                if (variant.DimensionValues != null && variant.DimensionValues.Count > 0)
                {
                    if (product.Dimensions == null || product.Dimensions.Count != variant.DimensionValues.Count)
                    {
                        return TypedResults.BadRequest("All product dimensions must be specified for the variant.");
                    }

                    if (product.Dimensions.Any(d => !variant.DimensionValues.Any(dv => dv.DimensionId == d.DimensionId)))
                    {
                        return TypedResults.BadRequest("All product dimensions must be specified for the variant.");
                    }

                    foreach (var dimValue in variant.DimensionValues)
                    {
                        dimValue.VariantId = variant.Id;
                    }
                    //await services.DbContext.VariantDimensionValues.AddRangeAsync(variant.DimensionValues);
                }
            }
        }


        // create outbox event
        product.Brand = brand;
        product.Category = category;
        var evt = product.ToProductCreatedEvent();

        await services.UnitOfWork.OutboxForLogTailingRepository.AddAsync(new LogTailingOutboxMessage()
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            PayloadType = typeof(ProductCatalog.Events.ProductCreatedEvent).FullName ?? throw new Exception($"Could not get fullname of type {evt.GetType()}"),
            Payload = JsonSerializer.Serialize(evt),
        });

        await services.UnitOfWork.DbContext.Products.AddAsync(product);
        foreach (var dimension in product.Dimensions ?? [])
        {
            await services.UnitOfWork.DbContext.ProductDimensions.AddAsync(new ProductDimension()
            {
                ProductId = product.Id,
                DimensionId = dimension.DimensionId
            });
        }

        await services.UnitOfWork.DbContext.SaveChangesAsync();
        await services.UnitOfWork.OutboxForLogTailingRepository.SaveChangesAsync();
        await services.UnitOfWork.CommitTransactionAsync(services.CancellationToken);

        return TypedResults.Ok(product);
    }

    private static async Task<Results<NotFound, Ok>> UpdateProduct([AsParameters] ApiServices services, Guid productId, Product product)
    {
        var existingProduct = await services.DbContext.Products.FindAsync(productId);
        if (existingProduct == null)
        {
            return TypedResults.NotFound();
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.IsActive = product.IsActive;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        existingProduct.CategoryId = product.CategoryId;

        services.DbContext.Products.Update(existingProduct);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
