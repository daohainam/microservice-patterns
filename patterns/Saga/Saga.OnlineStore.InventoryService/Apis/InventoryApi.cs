﻿using Saga.OnlineStore.InventoryService;
using Saga.OnlineStore.InventoryService.Infrastructure.Entity;
using Saga.OnlineStore.IntegrationEvents;

namespace Saga.OnlineStore.InventoryService.Apis;
public static class InventoryApi
{
    public static IEndpointRouteBuilder MapInventoryApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1/inventory")
              .MapInventoryApi()
              .WithTags("Inventory Api");

        return builder;
    }

    public static RouteGroupBuilder MapInventoryApi(this RouteGroupBuilder group)
    {
        group.MapGet("items", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Items.ToListAsync();
        });

        group.MapGet("items/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Items.FindAsync(id);
        });

        group.MapPut("items/{id:guid}/restock", Restock);

        return group;
    }

    private static async Task<Results<Ok<InventoryItem>, BadRequest>> CreateItem([AsParameters] ApiServices services, InventoryItem item)
    {
        if (item == null)
        {
            return TypedResults.BadRequest();
        }

        if (item.Id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var existingItem = await services.DbContext.Items.FindAsync(item.Id);
        if (existingItem != null)
        {
            return TypedResults.BadRequest();
        }

        await services.DbContext.Items.AddAsync(item);
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new ProductCreatedIntegrationEvent()
        {
            ProductId = item.Id,
        });

        return TypedResults.Ok(item);
    }

    public static async Task<Results<BadRequest, Ok>> Restock([AsParameters] ApiServices services, Guid id, RestockItem item)
    {
        if (item.Quantity <= 0)
        {
            return TypedResults.BadRequest();
        }

        var existingItem = await services.DbContext.Items.FindAsync(id);
        long quantityBefore = 0;
        long quantityAfter = item.Quantity;

        if (existingItem == null)
        {
            services.DbContext.Items.Add(new InventoryItem()
            {
                Id = id,
                AvailableQuantity = item.Quantity
            });
        }
        else
        {
            quantityBefore = existingItem.AvailableQuantity;

            existingItem.AvailableQuantity += item.Quantity;

            quantityAfter = existingItem.AvailableQuantity;

            services.DbContext.Items.Update(existingItem);
        }

        await services.DbContext.SaveChangesAsync();
        await services.EventPublisher.PublishAsync(new ItemRestockedIntegrationEvent()
        {
            ItemId = id,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
        });

        return TypedResults.Ok();
    }
}

public record RestockItem
{
    public long Quantity { get; set; }
}
