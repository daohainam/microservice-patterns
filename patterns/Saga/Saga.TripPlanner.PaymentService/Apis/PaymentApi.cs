using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Saga.TripPlanner.PaymentService.Apis;
public static class PaymentApiExtensions
{
    public static IEndpointRouteBuilder MapPaymentApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapPaymentApi()
              .WithTags("Payment Api");

        return builder;
    }

    public static RouteGroupBuilder MapPaymentApi(this RouteGroupBuilder group)
    {
        group.MapGet("cards", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Cards.ToListAsync();
        });

        group.MapGet("cards/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Cards.FindAsync(id);
        });

        group.MapPost("cards", PaymentApi.CreateCard);

        group.MapDelete("cards/{id:guid}", PaymentApi.DeleteCard);
        group.MapPut("cards/{id:guid}/deposit", PaymentApi.Deposit);
        return group;
    }
}
    public class PaymentApi
    {
        public static async Task<Results<Ok<CreditCard>, BadRequest>> CreateCard([AsParameters] ApiServices services, CreditCard card)
        {
            if (card == null)
            {
                return TypedResults.BadRequest();
            }

            if (card.Balance != 0)
            {
                services.Logger.LogError("New card must have balance = 0");
                return TypedResults.BadRequest();
            }

            if (card.Id == Guid.Empty)
                card.Id = Guid.CreateVersion7();

            var existingCard = await services.DbContext.Cards.Where(c => c.CardNumber == card.CardNumber).SingleOrDefaultAsync();
            if (existingCard != null)
            {
                services.Logger.LogError("Card already exists");
                return TypedResults.BadRequest();
            }

            await services.DbContext.Cards.AddAsync(card);
            await services.DbContext.SaveChangesAsync();

            await services.EventPublisher.PublishAsync(new CardCreatedIntegrationEvent()
            {
                CardId = card.Id,
                CardNumber = card.CardNumber,
                ExpirationDate = card.ExpirationDate,
                CardHolderName = card.CardHolderName,
                Cvv = card.Cvv
            });

            return TypedResults.Ok(card);
        }

        public static async Task<Results<NotFound, Ok>> DeleteCard([AsParameters] ApiServices services, Guid id)
        {
        var r = await services.DbContext.Cards.Where(c => c.Id == id).ExecuteDeleteAsync();
        if (r == 0)
        {
            return TypedResults.NotFound();
        }

        await services.EventPublisher.PublishAsync(new CardDeletedIntegrationEvent()
            {
                CardId = id
            });

            return TypedResults.Ok();
        }
    public static async Task<Results<NotFound, Ok, BadRequest>> Deposit([AsParameters] ApiServices services, Guid id, [FromBody]Deposit deposit)
        {
            if (deposit.Amount <= 0)
            {
                return TypedResults.BadRequest();
            }

            var existingCard = await services.DbContext.Cards.FindAsync(id);
            if (existingCard == null)
            {
                return TypedResults.NotFound();
            }

            existingCard.Balance += deposit.Amount;
            services.DbContext.Cards.Update(existingCard);

            await services.DbContext.SaveChangesAsync();
            await services.EventPublisher.PublishAsync(new CardBalanceChangedIntegrationEvent()
            {
                CardId = existingCard.Id,
                Balance = existingCard.Balance,
            });

            return TypedResults.Ok();
        }
    }

    public record Deposit
    {
        public decimal Amount { get; set; }
    }
