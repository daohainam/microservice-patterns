namespace IdempotentConsumer.CatalogService.Apis;
public static class CatalogApiExtensions
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/idempotentconsumer/v1")
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

        group.MapPost("products", CatalogApi.CreateProduct);

        group.MapPut("products/{id:guid}", CatalogApi.UpdateProduct);
        return group;
    }
}

public class CatalogApi
{
    internal static async Task<Results<Ok<Product>, BadRequest>> CreateProduct([AsParameters] ApiServices services, [FromHeader]Guid callId, Product product)
    {
        if (product == null || callId == Guid.Empty) {
            return TypedResults.BadRequest();
        }

        // Check if already processed (optimization to avoid unnecessary database operations)
        if (await services.DbContext.ProcessedMessages.AnyAsync(x => x.Id == callId))
        {
            services.Logger.LogInformation("Request with callId {CallId} already processed", callId);
            return TypedResults.Ok(product);
        }

        if (product.Id == Guid.Empty)
        {
            product.Id = Guid.CreateVersion7();
        }

        try
        {
            services.DbContext.ProcessedMessages.Add(new ProcessedMessage() { Id = callId, ProcessedAtUtc = DateTime.UtcNow });
            await services.DbContext.Products.AddAsync(product);
            await services.DbContext.SaveChangesAsync();

            return TypedResults.Ok(product);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Duplicate key violation - request was already processed by a concurrent call
            services.Logger.LogInformation("Request with callId {CallId} already processed (concurrent)", callId);
            return TypedResults.Ok(product);
        }
    }

    internal static async Task<Results<NotFound, Ok>> UpdateProduct([AsParameters] ApiServices services, [FromHeader] Guid callId, Guid id, Product product)
    {
        // Check if already processed (optimization to avoid unnecessary database operations)
        if (await services.DbContext.ProcessedMessages.AnyAsync(x => x.Id == callId))
        {
            return TypedResults.Ok();
        }

        var existingProduct = await services.DbContext.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return TypedResults.NotFound();
        }

        try
        {
            services.DbContext.ProcessedMessages.Add(new ProcessedMessage() { Id = callId, ProcessedAtUtc = DateTime.UtcNow });

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            services.DbContext.Products.Update(existingProduct);

            await services.DbContext.SaveChangesAsync();

            return TypedResults.Ok();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Duplicate key violation - request was already processed by a concurrent call
            return TypedResults.Ok();
        }
    }
}
