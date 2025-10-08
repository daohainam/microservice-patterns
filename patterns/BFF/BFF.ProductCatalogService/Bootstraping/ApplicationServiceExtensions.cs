using BFF.ProductCatalogService.Infrastructure.UoW;
using EventBus;
using EventBus.Abstractions;
using EventBus.Kafka;
using Npgsql;

namespace BFF.ProductCatalogService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<ProductCatalogDbContext>(Consts.DefaultDatabase, configureDbContextOptions: optionsBuilder =>
        {
            // You can add seed root category here if needed
            //optionsBuilder.UseSeeding((context, _) =>
            // {
            //     var rootCategory = context.Set<Category>().FirstOrDefault(c => c.Id == Guid.Empty);
            //     if (rootCategory == null)
            //     {
            //         context.Set<Category>().Add(new Category
            //         {
            //             Id = Guid.Empty,
            //             Name = "Root",
            //             UrlSlug = "",
            //             Description = "",
            //             ParentCategoryId = Guid.Empty,
            //             SortOrder = 0
            //         });
            //         context.SaveChanges();
            //     }
            // })
            //.UseAsyncSeeding(async (context, _, cancellationToken) =>
            //{
            //    var rootCategory = await context.Set<Category>().FirstOrDefaultAsync(c => c.Id == Guid.Empty, cancellationToken: cancellationToken);
            //    if (rootCategory == null)
            //    {
            //        context.Set<Category>().Add(new Category
            //        {
            //            Id = Guid.Empty,
            //            Name = "Root",
            //            UrlSlug = "",
            //            Description = "",
            //            ParentCategoryId = Guid.Empty,
            //            SortOrder = 0
            //        });

            //        await context.SaveChangesAsync(cancellationToken);
            //    }
            //});
        });

        builder.AddTransactionalOutbox(Consts.DefaultDatabase, options =>
        {
            options.PollingEnabled = false;
            options.PayloadAssembly = typeof(ProductCatalog.Events.ProductCreatedEvent).Assembly;
        });

        if (builder.Configuration.GetConnectionString(Consts.DefaultDatabase) is string connectionString)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
            {
                var connection = new NpgsqlConnection(connectionString);

                return new UnitOfWork(connection);
            });
        }
        else
        {
            throw new InvalidOperationException($"Connection string '{Consts.DefaultDatabase}' not found.");
        }

        builder.AddKafkaProducer("kafka");
        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrEmpty(kafkaTopic))
        {
            builder.AddKafkaEventPublisher(kafkaTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        return builder;
    }
}
