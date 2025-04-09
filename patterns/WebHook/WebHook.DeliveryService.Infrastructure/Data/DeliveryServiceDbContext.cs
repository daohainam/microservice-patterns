using Microsoft.EntityFrameworkCore;
using WebHook.DeliveryService.Infrastructure.Entity;

namespace WebHook.DeliveryService.Infrastructure.Data;
public class DeliveryServiceDbContext(DbContextOptions<DeliveryServiceDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries { get; set; } = default!;
    public DbSet<WebHookSubscription> WebHookSubscriptions { get; set; } = default!;
    public DbSet<DeliveryEventQueueItem> QueueItems { get; set; } = default!;
}
