namespace CQRS.Library.BookApi.Apis;
public static class BookApi
{
    public static IEndpointRouteBuilder MapBorrowerApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1")
              .MapBookApi()
              .WithTags("Book Api");

        return builder;
    }

    public static RouteGroupBuilder MapBookApi(this RouteGroupBuilder group)
    {
        group.MapGet("books", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Books.ToListAsync();
        });

        group.MapGet("books/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Books.FindAsync(id);
        });

        group.MapPost("books", CreateBorrower);

        group.MapPut("books/{id:guid}", UpdateBook);

        return group;
    }

    private static async Task<Results<Ok<Book>, BadRequest>> CreateBorrower([AsParameters] ApiServices services, Book borrower)
    {
        if (borrower == null) {
            return TypedResults.BadRequest();
        }

        if (borrower.Id == Guid.Empty)
            borrower.Id = Guid.CreateVersion7();

        await services.DbContext.Books.AddAsync(borrower);
        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new BookCreatedIntegrationEvent()
        {
        });

        return TypedResults.Ok(borrower);
    }

    private static async Task<Results<NotFound, Ok>> UpdateBook([AsParameters] ApiServices services, Guid id, Book borrower)
    {
        var existingBook = await services.DbContext.Books.FindAsync(id);
        if (existingBook == null)
        {
            return TypedResults.NotFound();
        }
        existingBook.Title = borrower.Title;
        existingBook.Author = borrower.Author;
        services.DbContext.Books.Update(existingBook);

        await services.DbContext.SaveChangesAsync();
        await services.EventPublisher.PublishAsync(new BookUpdatedIntegrationEvent()
        {
            BookId = existingBook.Id,
            Title = existingBook.Title,
            Author = existingBook.Author,
        });

        return TypedResults.Ok();
    }
}
