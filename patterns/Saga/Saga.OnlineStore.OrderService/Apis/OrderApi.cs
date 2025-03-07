using Saga.OnlineStore.OrderService;
using Saga.OnlineStore.OrderService.Infrastructure.Entity;
using Saga.OnlineStore.IntegrationEvents;
using OrderStatus = Saga.OnlineStore.OrderService.Infrastructure.Entity.OrderStatus;

namespace Saga.OnlineStore.OrderService.Apis;
public static class OrderApi
{
    public static IEndpointRouteBuilder MapOrderApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapOrderApi()
              .WithTags("Order Api");

        return builder;
    }

    public static RouteGroupBuilder MapOrderApi(this RouteGroupBuilder group)
    {
        group.MapGet("orders", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Orders.Include(o => o.Items).ToListAsync();
        });

        group.MapGet("orders/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Orders.Include(o => o.Items).Where(o => o.Id == id).FirstOrDefaultAsync();
        });

        group.MapPost("orders", PlaceOrder);

        return group;
    }

    private static async Task<Results<Ok<Order>, BadRequest>> PlaceOrder([AsParameters] ApiServices services, Order order)
    {
        if (order == null) {
            return TypedResults.BadRequest();
        }

        if (order.Id == Guid.Empty)
            order.Id = Guid.CreateVersion7();

        order.Status = OrderStatus.Pending;
        order.StatusMessage = "Pending";

        await services.DbContext.Orders.AddAsync(order);
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new OrderPlacedIntegrationEvent()
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            PaymentCardNumber = order.PaymentCardNumber,
            PaymentCardName = order.PaymentCardName,
            PaymentCardExpiration = order.PaymentCardExpiration,
            PaymentCardCvv = order.PaymentCardCvv,
            Items = [.. order.Items.Select(i => new IntegrationEvents.OrderItem()
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.UnitPrice,
            })],
        });

        return TypedResults.Ok(order);
    }
}
