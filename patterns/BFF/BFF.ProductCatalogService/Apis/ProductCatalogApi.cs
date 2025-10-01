using BFF.ProductCatalogService.Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc;

namespace BFF.ProductCatalogService.Apis;
public static class ProductCatalogApi
{
    private static readonly string InvalidDisplayType = $"Invalid display type. Valid types are: {string.Join(", ", DimensionDisplayTypes.All)}";

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

        group.MapPost("dimensions", AddDimentions);
        group.MapPost("dimensions/{id}/values", AddDimentionValues);

        group.MapPost("categories", AddCategory)

        return group;
    }

    private static async Task<Results<Ok<Category>, BadRequest>> AddCategory([AsParameters] ApiServices services, Category category)
    {
        if (category == null)
        {
            return TypedResults.BadRequest();
        }
        if (category.Id == Guid.Empty)
            category.Id = Guid.CreateVersion7();
        await services.DbContext.Categories.AddAsync(category);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(category);
    }

    private static async Task<Results<Ok<DimensionValue>, BadRequest, BadRequest<string>>> AddDimentionValues([AsParameters] ApiServices services, [FromRoute] string id, DimensionValue dimensionValue)
    {
        if (dimensionValue == null)
        {
            return TypedResults.BadRequest();
        }

        if (dimensionValue.Id == Guid.Empty)
            dimensionValue.Id = Guid.CreateVersion7();

        dimensionValue.DimensionId = id;

        await services.DbContext.DimensionValues.AddAsync(dimensionValue);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(dimensionValue);
    }

    private static async Task<Results<Ok<Dimension>, BadRequest, BadRequest<string>>> AddDimentions([AsParameters] ApiServices services, Dimension dimension)
    {
        if (dimension == null)
        {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrEmpty(dimension.Id))
            return TypedResults.BadRequest("Dimension Id is required.");
        if (string.IsNullOrEmpty(dimension.DisplayType))
            dimension.DisplayType = DimensionDisplayTypes.Text;
        else if (!DimensionDisplayTypes.All.Contains(dimension.DisplayType))
            return TypedResults.BadRequest(InvalidDisplayType);

        if (!IsValidDimensionId(dimension.Id))
        {
            return TypedResults.BadRequest("Dimension Id can only contain alphanumeric characters and underscores.");
        }

        await services.DbContext.Dimensions.AddAsync(dimension);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(dimension);
    }

    private static bool IsValidDimensionId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (!char.IsLetter(id.First()))
        {
            return false;
        }

        return id.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private static async Task<Results<Ok<Product>, BadRequest>> CreateProduct([AsParameters] ApiServices services, Product product)
    {
        if (product == null)
        {
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
