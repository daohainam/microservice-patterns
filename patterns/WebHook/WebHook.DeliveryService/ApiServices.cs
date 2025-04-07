using WebHook.DeliveryService.Apis;
using WebHook.DeliveryService.Infrastructure.Data;

namespace WebHook.DeliveryService;
public class ApiServices(
    DeliveryServiceApiDbContext dbContext,
    ILogger<DeliveryServiceApi> logger)
{
    public DeliveryServiceApiDbContext DbContext => dbContext;
    public ILogger<DeliveryServiceApi> Logger => logger;
}
