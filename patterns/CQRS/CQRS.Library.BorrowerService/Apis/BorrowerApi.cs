using CQRS.Library.BorrowerService;
using CQRS.Library.BorrowerService.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.Library.BorrowerService.Apis;
public static class BorrowerApi
{
    public static IEndpointRouteBuilder MapBorrowerApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1")
              .MapBorrowerApi()
              .WithTags("Borrower Api");

        return builder;
    }

    public static RouteGroupBuilder MapBorrowerApi(this RouteGroupBuilder group)
    {
        group.MapGet("borrowers", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Borrowers.ToListAsync();
        });

        group.MapGet("borrowers/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Borrowers.FindAsync(id);
        });

        group.MapPost("borrowers", CreateBorrower);

        group.MapPut("borrowers/{id:guid}", UpdateBorrower);

        return group;
    }

    private static async Task<Results<Ok<Borrower>, BadRequest>> CreateBorrower([AsParameters] ApiServices services, Borrower borrower)
    {
        if (borrower == null) {
            return TypedResults.BadRequest();
        }

        if (borrower.Id == Guid.Empty)
            borrower.Id = Guid.CreateVersion7();

        await services.DbContext.Borrowers.AddAsync(borrower);
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BorrowerCreatedIntegrationEvent()
        {
            BorrowerId = borrower.Id,
            Name = borrower.Name,
            Address = borrower.Address,
            PhoneNumber = borrower.PhoneNumber,
            Email = borrower.Email
        });

        return TypedResults.Ok(borrower);
    }

    private static async Task<Results<NotFound, Ok>> UpdateBorrower([AsParameters] ApiServices services, Guid id, Borrower borrower)
    {
        var existingBorrower = await services.DbContext.Borrowers.FindAsync(id);
        if (existingBorrower == null)
        {
            return TypedResults.NotFound();
        }
        existingBorrower.Name = borrower.Name;
        existingBorrower.Address = borrower.Address;
        existingBorrower.PhoneNumber = borrower.PhoneNumber;
        existingBorrower.Email = borrower.Email;
        services.DbContext.Borrowers.Update(existingBorrower);

        await services.DbContext.SaveChangesAsync();
        await services.EventPublisher.PublishAsync(new BorrowerUpdatedIntegrationEvent()
        {
            BorrowerId = existingBorrower.Id,
            Name = existingBorrower.Name,
            Address = existingBorrower.Address,
            PhoneNumber = existingBorrower.PhoneNumber,
            Email = existingBorrower.Email
        });

        return TypedResults.Ok();
    }
}
