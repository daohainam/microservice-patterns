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
            return await services.DbContext.CreditCards.ToListAsync();
        });

        group.MapGet("cards/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.CreditCards.FindAsync(id);
        });

        group.MapPost("cards", PaymentApi.CreateCard);

        group.MapDelete("cards/{id:guid}", PaymentApi.DeleteCard);
        group.MapPut("cards/{id:guid}/pay", PaymentApi.Pay);
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

        if (card.CreditLimit < 0)
        {
            services.Logger.LogError("Credit limit must be greater than or equal 0");
            return TypedResults.BadRequest();
        }

        if (card.AvailableCredit != card.CreditLimit)
        {
            services.Logger.LogError("Available credit must be equal to credit limit");
            return TypedResults.BadRequest();
        }

        if (card.Id == Guid.Empty)
            card.Id = Guid.CreateVersion7();

        var existingCard = await services.DbContext.CreditCards.Where(c => c.CardNumber == card.CardNumber).SingleOrDefaultAsync();
        if (existingCard != null)
        {
            services.Logger.LogError("Card already exists");
            return TypedResults.BadRequest();
        }

        await services.DbContext.CreditCards.AddAsync(card);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(card);
    }

    public static async Task<Results<NotFound, Ok>> DeleteCard([AsParameters] ApiServices services, Guid id)
    {
        var r = await services.DbContext.CreditCards.Where(c => c.Id == id).ExecuteDeleteAsync();
        if (r == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }
    public static async Task<Results<NotFound, Ok, BadRequest>> Pay([AsParameters] ApiServices services, Guid id, [FromBody] CreditCardPayment payment)
    {
        if (payment.Amount <= 0)
        {
            services.Logger.LogError("Payment amount must be greater than 0");
            return TypedResults.BadRequest();
        }

        var existingCard = await services.DbContext.CreditCards.FindAsync(id);
        if (existingCard == null)
        {
            return TypedResults.NotFound();
        }

        var availableCredit = existingCard.AvailableCredit + payment.Amount; // we allow overpayment

        existingCard.AvailableCredit = availableCredit;
        services.DbContext.CreditCards.Update(existingCard);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}

public record CreditCardPayment
{
    public decimal Amount { get; set; }
}
