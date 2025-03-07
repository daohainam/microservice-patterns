using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;
public class BookBorrowingIntegrationEventHandler(BorrowingHistoryDbContext dbContext, ILogger<BookBorrowingIntegrationEventHandler> logger) :
    INotificationHandler<BookBorrowedIntegrationEvent>,
    INotificationHandler<BookReturnedIntegrationEvent>
{
    public async Task Handle(BookBorrowedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling book borrowed event: {bookId}", request.BookId);

        var book = await dbContext.Books.Where(book => book.Id == request.BookId).SingleOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Book not found");
        var borrower = await dbContext.Borrowers.Where(borrower => borrower.Id == request.BorrowerId).SingleOrDefaultAsync(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Borrower not found");

        dbContext.BorrowingHistoryItems.Add(
            new BorrowingHistoryItem
            {
                Id = request.BorrowingId,
                BookId = request.BookId,
                BorrowerId = request.BorrowerId,
                BorrowedAt = request.BorrowedAt,
                ValidUntil = request.ValidUntil,
                HasReturned = false,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BorrowerName = borrower.Name,
                BorrowerAddress = borrower.Address,
                BorrowerEmail = borrower.Email,
                BorrowerPhoneNumber = borrower.PhoneNumber
            }
        );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(BookReturnedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling borrowing updated event: {id}", request.BorrowingId);

        await dbContext.BorrowingHistoryItems.Where(x => x.Id == request.BorrowingId).ExecuteUpdateAsync(
            setters => setters
            .SetProperty(b => b.HasReturned, true)
            .SetProperty(b => b.ReturnedAt, request.ReturnedAt), cancellationToken: cancellationToken);
    }
}
