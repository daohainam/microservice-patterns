namespace CQRS.Library.BorrowingService.Apis;
public static class BorrowingApi
{
    public static IEndpointRouteBuilder MapBorrowingApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1")
              .MapBorrowerApi()
              .WithTags("Borrowing Api");

        return builder;
    }

    public static RouteGroupBuilder MapBorrowerApi(this RouteGroupBuilder group)
    {
        group.MapPost("borrowings", BorrowBook);
        group.MapPut("borrowings/{id:guid}/return", ReturnBook);

        return group;
    }

    private static async Task<Results<Ok<Borrowing>, BadRequest<string>>> BorrowBook([AsParameters] ApiServices services, [Required] Borrowing borrowing)
    {
        if (borrowing.BorrowerId == Guid.Empty)
        {
            return TypedResults.BadRequest("BorrowerId is required");
        }

        if (borrowing.BookId == Guid.Empty)
        {
            return TypedResults.BadRequest("BookId is required");
        }

        borrowing.Id = Guid.CreateVersion7();
        borrowing.BorrowedAt = DateTime.UtcNow;
        borrowing.ValidUntil = borrowing.ValidUntil.ToUniversalTime();
        borrowing.HasReturned = false;

        await services.DbContext.Borrowings.AddAsync(borrowing);
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BookBorrowedIntegrationEvent()
        {
            BorrowingId = borrowing.Id,
            BorrowerId = borrowing.BorrowerId,
            BookId = borrowing.BookId,
            BorrowedAt = borrowing.BorrowedAt,
            ValidUntil = borrowing.ValidUntil
        });

        return TypedResults.Ok(borrowing);
    }

    private static async Task<Results<Ok, NotFound>> ReturnBook([AsParameters] ApiServices services, Guid id)
    {
        var borrowing = await services.DbContext.Borrowings.FindAsync(id);
        if (borrowing == null)
        {
            return TypedResults.NotFound();
        }

        borrowing.ReturnedAt = DateTime.UtcNow;
        borrowing.HasReturned = true;
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BookReturnedIntegrationEvent()
        {
            BorrowingId = borrowing.Id,
            BorrowerId = borrowing.BorrowerId,
            BookId = borrowing.BookId,
            ReturnedAt = borrowing.ReturnedAt.Value
        });

        return TypedResults.Ok();
    }
}
