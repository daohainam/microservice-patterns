using WebHook.DeliveryService.Infrastructure.Entity;

namespace WebHook.DeliveryService.Infrastructure.Data;
public class DeliveryServiceApiDbContext(DbContextOptions<DeliveryServiceApiDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries { get; set; } = default!;
    public DbSet<WebHook.DeliveryService.Infrastructure.Entity.WebHook> WebHooks { get; set; } = default!;
    public DbSet<WebHookEventType> WebHookEventTypes { get; set; } = default!;
    public DbSet<WebHookQueueItem> WebHookQueueItems { get; set; } = default!;
}
