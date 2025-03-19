using MicroservicePatterns.Shared;
using Microsoft.Extensions.Configuration;

namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis");
        var kafka = builder.AddKafka("kafka");
        var mongoDb = builder.AddMongoDB("mongodb"); 
        var postgres = builder.AddPostgres("postgresql");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            cache = cache.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithRedisInsight();
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
            mongoDb = mongoDb.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithMongoExpress();
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithPgWeb();
        }

        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });

        #region CQRS Library
        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        var bookService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BookService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BookService>())
            .WithReference(kafka)
            .WithReference(bookDb, Consts.DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowerDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowerService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowerService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, Consts.DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka)
            .WithParentRelationship(bookService);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, Consts.DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka)
            .WithParentRelationship(bookService);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingHistoryService>()
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.CQRS_Library_BorrowerService>(),
                    GetTopicName<Projects.CQRS_Library_BookService>(),
                    GetTopicName<Projects.CQRS_Library_BorrowingService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb, Consts.DefaultDatabase)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka)
            .WithParentRelationship(bookService);
        #endregion CQRS Library

        #region Saga Online Store - Choreography
        var sagaOrderDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_OrderService>();
        var sagaOrderService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_OrderService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_OrderService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaOrderDb, Consts.DefaultDatabase)
            .WaitFor(sagaOrderDb)
            .WaitFor(kafka);

        
        var sagaCatalogDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_CatalogService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_CatalogService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_CatalogService>())
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, Consts.DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka)
            .WithParentRelationship(sagaOrderService);

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_InventoryService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_InventoryService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_CatalogService>(),
                    GetTopicName<Projects.Saga_OnlineStore_OrderService>(),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, Consts.DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka)
            .WithParentRelationship(sagaOrderService);

        var sagaBankCardDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_PaymentService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_PaymentService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_PaymentService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, Consts.DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka)
            .WithParentRelationship(sagaOrderService);

        #endregion

        #region Saga Trip Planner - Orchestration
        var sagaHotelDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_HotelService>();
        var sagaHotelService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_HotelService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_HotelService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaHotelDb, Consts.DefaultDatabase)
            .WaitFor(sagaHotelDb)
            .WaitFor(kafka);

        var sagaTicketlDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_TicketService>();
        var sagaTicketService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_TicketService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_TicketService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaTicketlDb, Consts.DefaultDatabase)
            .WaitFor(sagaTicketlDb)
            .WaitFor(kafka);

        var sagaPaymentDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_PaymentService>();
        var sagaPaymentService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_PaymentService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_PaymentService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaPaymentDb, Consts.DefaultDatabase)
            .WaitFor(sagaPaymentDb)
            .WaitFor(kafka);

        var sagaTripPlanningDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_TripPlanningService>();
        var sagaTripPlanningService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_TripPlanningService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaTripPlanningDb, Consts.DefaultDatabase)
            .WaitFor(sagaTripPlanningDb)
            .WaitFor(kafka);
        #endregion

        #region Event Sourcing Catalog
        //builder.AddProject<Projects.EventSourcing_Catalog_ProductService>("eventsourcing-catalog-productservice");
        #endregion

        return builder;
    }

    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();

        TopicSpecification[] topics = [
            new() { Name = GetTopicName<Projects.CQRS_Library_BookService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowerService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowingService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_CatalogService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_PaymentService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_InventoryService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_OrderService>(), NumPartitions = 1, ReplicationFactor = 1 }
        ];

        logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));

        var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = connectionString,
        }).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions() { });
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "An error occurred creating topics");
        }
    }

    private static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";
}
