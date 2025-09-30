using BFF.ProductCatalogService.Infrastructure.Entity;

namespace BFF.ProductCatalogService.Apis;
public static class ProductCatalogApi
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/bff/v1")
              .MapCatalogApi()
              .WithTags("Product Catalog Api");

        return builder;
    }

    public static RouteGroupBuilder MapCatalogApi(this RouteGroupBuilder group)
    {
        group.MapGet("products", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Products.ToListAsync();
        });
        group.MapGet("products/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Products.FindAsync(id);
        });
        group.MapPost("products", CreateProduct);
        group.MapPut("products/{id:guid}", UpdateProduct);

        group.MapPost("products/{id:guid}/dimensions", AddDimentions);

        return group;
    }
    private static async Task<Results<Ok<Dimension>, BadRequest>> AddDimentions([AsParameters] ApiServices services, Dimension dimension)
    {
        if (dimension == null) {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrEmpty(dimension.Id))
            dimension.Id = Guid.NewGuid().ToString();
        await services.DbContext.Dimensions.AddAsync(dimension);
        await services.DbContext.SaveChangesAsync();
        return TypedResults.Ok(dimension);
    }

    private static async Task<Results<Ok<Product>, BadRequest>> CreateProduct([AsParameters] ApiServices services, Product product)
    {
        if (product == null) {
            return TypedResults.BadRequest();
        }

        if (product.Id == Guid.Empty)
            product.Id = Guid.CreateVersion7();

        await services.DbContext.Products.AddAsync(product);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(product);
    }

    private static async Task<Results<NotFound, Ok>> UpdateProduct([AsParameters] ApiServices services, Guid id, Product product)
    {
        var existingProduct = await services.DbContext.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return TypedResults.NotFound();
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        services.DbContext.Products.Update(existingProduct);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
