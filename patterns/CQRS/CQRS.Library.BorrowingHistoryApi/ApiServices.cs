using EventBus.Abstractions;

namespace CQRS.Library.BorrowingHistoryApi;
public class ApiServices(
    BorrowingDbContext dbContext)
{
    public BorrowingDbContext DbContext => dbContext;

}
