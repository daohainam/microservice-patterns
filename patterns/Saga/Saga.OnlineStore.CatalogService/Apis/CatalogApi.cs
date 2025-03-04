using Saga.OnlineStore.CatalogService;
using Saga.OnlineStore.CatalogService.Infrastructure.Entity;
using Saga.OnlineStore.IntegrationEvents;

namespace Saga.OnlineStore.CatalogService.Apis;
public static class CatalogApi
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapCatalogApi()
              .WithTags("Catalog Api");

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
        return group;
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

        await services.EventPublisher.PublishAsync(new ProductCreatedIntegrationEvent()
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
        });

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
        existingProduct.Price = product.Price;
        services.DbContext.Products.Update(existingProduct);

        await services.DbContext.SaveChangesAsync();
        await services.EventPublisher.PublishAsync(new ProductUpdatedIntegrationEvent()
        {
            ProductId = existingProduct.Id,
            Name = existingProduct.Name,
            Description = existingProduct.Description,
            Price = existingProduct.Price,
        });

        return TypedResults.Ok();
    }
}
