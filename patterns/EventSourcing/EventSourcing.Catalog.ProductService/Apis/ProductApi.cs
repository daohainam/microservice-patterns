namespace EventSourcing.Catalog.ProductService.Apis;
public static class ProductApi
{
    public static IEndpointRouteBuilder MapProductApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/eventsourcing/v1")
              .MapBookApi()
              .WithTags("Product Api");

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

        group.MapPost("books", CreateBook);

        group.MapPut("books/{id:guid}", UpdateBook);

        return group;
    }

    private static async Task<Results<Ok<Product>, BadRequest>> CreateBook([AsParameters] ApiServices services, Product book)
    {
        if (book == null) {
            return TypedResults.BadRequest();
        }

        if (book.Id == Guid.Empty)
            book.Id = Guid.CreateVersion7();

        await services.DbContext.Books.AddAsync(book);
        await services.DbContext.SaveChangesAsync();

        //await services.EventPublisher.PublishAsync(new BookCreatedIntegrationEvent()
        //{
        //    BookId = book.Id,
        //    Title = book.Title,
        //    Author = book.Author,
        //});

        return TypedResults.Ok(book);
    }

    private static async Task<Results<NotFound, Ok>> UpdateBook([AsParameters] ApiServices services, Guid id, Product book)
    {
        var existingBook = await services.DbContext.Books.FindAsync(id);
        if (existingBook == null)
        {
            return TypedResults.NotFound();
        }
        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        services.DbContext.Books.Update(existingBook);

        await services.DbContext.SaveChangesAsync();
        //await services.EventPublisher.PublishAsync(new BookUpdatedIntegrationEvent()
        //{
        //    BookId = existingBook.Id,
        //    Title = existingBook.Title,
        //    Author = existingBook.Author,
        //});

        return TypedResults.Ok();
    }
}
