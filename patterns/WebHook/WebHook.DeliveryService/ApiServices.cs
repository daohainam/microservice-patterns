namespace WebHook.DeliveryService;
public class ApiServices(
    DeliveryServiceDbContext dbContext,
    ILogger<DeliveryServiceApi> logger,
    ISecretKeyService secretKeyService)
{
    public DeliveryServiceDbContext DbContext => dbContext;
    public ILogger<DeliveryServiceApi> Logger => logger;
    public ISecretKeyService SecretKeyService => secretKeyService;
}
