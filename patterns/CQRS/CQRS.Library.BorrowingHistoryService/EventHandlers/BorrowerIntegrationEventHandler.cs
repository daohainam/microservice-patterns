using CQRS.Library.BorrowingHistoryService.Infrastructure.Data;

namespace CQRS.Library.BorrowingHistoryService.EventHandlers;
public class BorrowerIntegrationEventHandler(BorrowingHistoryDbContext dbContext, ILogger<BorrowerIntegrationEventHandler> logger) :
    IRequestHandler<BorrowerCreatedIntegrationEvent>, 
    IRequestHandler<BorrowerUpdatedIntegrationEvent>
{
    public async Task Handle(BorrowerCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling borrower created event: {BorrowerId}", request.BorrowerId);

        var borrower = new Borrower
        {
            Id = request.BorrowerId,
            Name = request.Name,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email
        };

        dbContext.Borrowers.Add(borrower);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(BorrowerUpdatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling borrower updated event: {BorrowerId}", request.BorrowerId);
        
        await dbContext.Borrowers.Where(x => x.Id == request.BorrowerId).ExecuteUpdateAsync(
            setters => setters.SetProperty(b => b.Address, request.Address)
            .SetProperty(b => b.Email, request.Email)
            .SetProperty(b => b.Name, request.Name)
            .SetProperty(b => b.PhoneNumber, request.PhoneNumber),
            cancellationToken: cancellationToken);

        await dbContext.BorrowingHistoryItems.Where(x => x.BookId == request.BorrowerId).ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.BorrowerAddress, request.Address)
        .SetProperty(b => b.BorrowerEmail, request.Email)
        .SetProperty(b => b.BorrowerName, request.Name)
        .SetProperty(b => b.BorrowerPhoneNumber, request.PhoneNumber),
        cancellationToken: cancellationToken);
    }
}
