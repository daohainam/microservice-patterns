using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;
public class BookIntegrationEventHandler(BorrowingHistoryDbContext dbContext, ILogger<BookIntegrationEventHandler> logger) :
    IRequestHandler<BookCreatedIntegrationEvent>,
    IRequestHandler<BookUpdatedIntegrationEvent>
{
    public async Task Handle(BookCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book created event: {bookId}", request.BookId);
        dbContext.Books.Add(new Book
        {
            Id = request.BookId,
            Title = request.Title,
            Author = request.Author
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(BookUpdatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book updated event: {bookId}", request.BookId);
        await dbContext.Books.Where(x => x.Id == request.BookId).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Title, request.Title).SetProperty(b => b.Author, request.Author), cancellationToken: cancellationToken);
        await dbContext.BorrowingHistoryItems.Where(x => x.BookId == request.BookId).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.BookTitle, request.Title).SetProperty(b => b.BookAuthor, request.Author), cancellationToken: cancellationToken);
    }
}
