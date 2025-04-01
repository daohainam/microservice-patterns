using TransactionalOutbox.Infrastructure;
using TransactionalOutbox.Banking.AccountService.Apis;

namespace TransactionalOutbox.Banking.AccountService;
public class ApiServices(
    IUnitOfWork unitOfWork,
    ILogger<AccountApi> logger,
    CancellationToken cancellationToken)
{
    public IUnitOfWork UnitOfWork => unitOfWork;
    public ILogger<AccountApi> Logger => logger;
    public CancellationToken CancellationToken => cancellationToken;

}
