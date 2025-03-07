namespace Saga.OnlineStore.PaymentService.Apis;
public static class PaymentApi
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

        group.MapPost("cards", CreateCard);

        group.MapPut("cards/{id:guid}", UpdateCard);
        group.MapPut("cards/{id:guid}/deposit", Deposit);
        return group;
    }

    private static async Task<Results<Ok<Card>, BadRequest>> CreateCard([AsParameters] ApiServices services, Card card)
    {
        if (card == null) {
            return TypedResults.BadRequest();
        }

        if (card.Id == Guid.Empty)
            card.Id = Guid.CreateVersion7();

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

    private static async Task<Results<NotFound, Ok>> UpdateCard([AsParameters] ApiServices services, Guid id, Card card)
    {
        var existingCard = await services.DbContext.Cards.FindAsync(id);
        if (existingCard == null)
        {
            return TypedResults.NotFound();
        }

        existingCard.CardHolderName = card.CardHolderName;
        existingCard.CardNumber = card.CardNumber;
        existingCard.ExpirationDate = card.ExpirationDate;
        existingCard.Cvv = card.Cvv;

        services.DbContext.Cards.Update(existingCard);

        await services.DbContext.SaveChangesAsync();
        await services.EventPublisher.PublishAsync(new CardUpdatedIntegrationEvent()
        {
            CardId = existingCard.Id,
            CardNumber = existingCard.CardNumber,
            ExpirationDate = existingCard.ExpirationDate,
            CardHolderName = existingCard.CardHolderName,
            Cvv = existingCard.Cvv
        });

        return TypedResults.Ok();
    }
    private static async Task<Results<NotFound, Ok, BadRequest>> Deposit([AsParameters] ApiServices services, Guid id, Deposit deposit)
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
