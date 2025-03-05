namespace Saga.OnlineStore.BankCardService.Apis;
public static class BankCardApi
{
    public static IEndpointRouteBuilder MapBankCardApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapCatalogApi()
              .WithTags("BankCard Api");

        return builder;
    }

    public static RouteGroupBuilder MapCatalogApi(this RouteGroupBuilder group)
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

        group.MapPut("cards/{id:guid}", UpdateProduct);
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

        await services.EventPublisher.PublishAsync(new ProductCreatedIntegrationEvent()
        {
            ProductId = card.Id,
        });

        return TypedResults.Ok(card);
    }

    private static async Task<Results<NotFound, Ok>> UpdateProduct([AsParameters] ApiServices services, Guid id, Card card)
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
}
