namespace WebHook.DeliveryService.Apis;
public static class DeliveryServiceApiExtensions
{
    public static IEndpointRouteBuilder MapDeliveryServiceApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/webhook/v1")
              .MapDeliveryServiceApi()
              .WithTags("Delivery Service Api");

        return builder;
    }
    public static RouteGroupBuilder MapDeliveryServiceApi(this RouteGroupBuilder group)
    {
        group.MapGet("deliveries", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Deliveries.ToListAsync();
        });

        group.MapGet("deliveries/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Deliveries.FindAsync(id);
        });

        group.MapPost("deliveries", DeliveryServiceApi.CreateDelivery);

        group.MapPut("deliveries/{id:guid}", DeliveryServiceApi.UpdateDelivery);

        return group;
    }

}

public class DeliveryServiceApi
{ 
    internal static async Task<Results<Ok<Delivery>, BadRequest>> CreateDelivery([AsParameters] ApiServices services, Delivery delivery)
    {
        if (delivery == null) {
            return TypedResults.BadRequest();
        }

        if (delivery.Id == Guid.Empty)
            delivery.Id = Guid.CreateVersion7();

        await services.DbContext.Deliveries.AddAsync(delivery);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(delivery);
    }

    internal static async Task<Results<NotFound, Ok>> UpdateDelivery([AsParameters] ApiServices services, Guid id, Delivery delivery)
    {
        var existingDelivery = await services.DbContext.Deliveries.FindAsync(id);
        if (existingDelivery == null)
        {
            return TypedResults.NotFound();
        }
        existingDelivery.Title = delivery.Title;
        existingDelivery.Author = delivery.Author;
        services.DbContext.Deliveries.Update(existingDelivery);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
