using WebHook.DeliveryService.Apis;
using WebHook.DeliveryService.Infrastructure.Data;

namespace WebHook.DeliveryService;
public class ApiServices(
    DeliveryServiceDbContext dbContext,
    ILogger<DeliveryServiceApi> logger)
{
    public DeliveryServiceDbContext DbContext => dbContext;
    public ILogger<DeliveryServiceApi> Logger => logger;
}
