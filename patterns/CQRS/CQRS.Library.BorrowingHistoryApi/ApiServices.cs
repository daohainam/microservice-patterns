using EventBus.Abstractions;

namespace CQRS.Library.BorrowingHistoryApi;
public class ApiServices(
    BorrowingHistoryDbContext dbContext)
{
    public BorrowingHistoryDbContext DbContext => dbContext;

}
