using System.Text.Json;
using WebHook.DeliveryService.DomainEvents;

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

        var deliveryCreatedEvent = new DeliveryCreatedEvent()
        {
            CreatedAt = DateTime.UtcNow,
            Id = delivery.Id,
            Sender = delivery.Sender,
            Receiver = delivery.Receiver,
            SenderAddress = delivery.SenderAddress,
            ReceiverAddress = delivery.ReceiverAddress,
            PackageInfo = delivery.PackageInfo,
        };
        var deliveryCreatedEventJson = JsonSerializer.Serialize(deliveryCreatedEvent);

        foreach (var subcribtion in services.DbContext.WebHookSubscriptions)
        {
            var queueItem = new DeliveryEventQueueItem
            {
                Id = Guid.CreateVersion7(),
                WebHookSubscriptionId = subcribtion.Id,
                Message = deliveryCreatedEventJson,
                MessageType = typeof(DeliveryCreatedEvent).FullName,
                MessageSource = "https://localhost:7143",
                CreatedAt = DateTime.UtcNow,
                ScheduledAt = DateTime.UtcNow,
                IsProcessed = false,
                IsSuccess = false,
                RetryTimes = 0,
            };
            await services.DbContext.QueueItems.AddAsync(queueItem);
        }

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
        existingDelivery.Sender = delivery.Sender;
        existingDelivery.Receiver = delivery.Receiver;
        existingDelivery.SenderAddress = delivery.SenderAddress;
        existingDelivery.ReceiverAddress = delivery.ReceiverAddress;

        services.DbContext.Deliveries.Update(existingDelivery);

        var deliveryUpdatedEvent = new DeliveryUpdatedEvent()
        {
            CreatedAt = DateTime.UtcNow,
            Id = delivery.Id,
            Sender = delivery.Sender,
            Receiver = delivery.Receiver,
            SenderAddress = delivery.SenderAddress,
            ReceiverAddress = delivery.ReceiverAddress,
            PackageInfo = delivery.PackageInfo,
        };
        var deliveryUpdatedEventJson = JsonSerializer.Serialize(deliveryUpdatedEvent);

        foreach (var subcription in services.DbContext.WebHookSubscriptions)
        {
            var queueItem = new DeliveryEventQueueItem
            {
                Id = Guid.CreateVersion7(),
                WebHookSubscriptionId = subcription.Id,
                Message = deliveryUpdatedEventJson,
                MessageType = typeof(DeliveryUpdatedEvent).FullName,
                MessageSource = "https://localhost:7143",
                CreatedAt = DateTime.UtcNow,
                ScheduledAt = DateTime.UtcNow,
                IsProcessed = false,
                IsSuccess = false,
                RetryTimes = 0,
            };
            await services.DbContext.QueueItems.AddAsync(queueItem);
        }


        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}
