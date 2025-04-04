using MicroservicePatterns.Shared;
using Microsoft.Extensions.Configuration;
using TransactionalOutbox.Publisher.Debezium;

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
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI().WithDebezium();
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
        var borrowerService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowerService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, Consts.DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        var borrowingService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, Consts.DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        var borrowingHistoryService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingHistoryService>()
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
            .WaitFor(kafka);

        bookService.WithParentRelationship(borrowingHistoryService);
        borrowerService.WithParentRelationship(borrowingHistoryService);
        borrowingService.WithParentRelationship(borrowingHistoryService);
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
        var sagaCatalogService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_CatalogService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_CatalogService>())
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, Consts.DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        var sagaInventoryService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_InventoryService>()
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
        var sagaBankCardService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_PaymentService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_PaymentService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, Consts.DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka);


        sagaBankCardService.WithParentRelationship(sagaOrderService);
        sagaCatalogService.WithParentRelationship(sagaOrderService);
        sagaInventoryService.WithParentRelationship(sagaOrderService);
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
            .WithReference(sagaPaymentService)
            .WithReference(sagaTicketService)
            .WithReference(sagaHotelService)
            .WaitFor(sagaTripPlanningDb)
            .WaitFor(kafka);

        sagaHotelService.WithParentRelationship(sagaTripPlanningService);
        sagaTicketService.WithParentRelationship(sagaTripPlanningService);
        sagaPaymentService.WithParentRelationship(sagaTripPlanningService);
        #endregion

        #region Event Sourcing Account

        var esAccountDb = postgres.AddDefaultDatabase<Projects.EventSourcing_Banking_AccountService>();
        var esAccountkService = builder.AddProjectWithPostfix<Projects.EventSourcing_Banking_AccountService>()
            .WithReference(esAccountDb, Consts.DefaultDatabase)
            .WaitFor(esAccountDb);

        #endregion

        #region Transactional Outbox Account
        var outboxAccountDb = postgres.AddDefaultDatabase<Projects.TransactionalOutbox_Banking_AccountService>();

        var outboxAccountService = builder.AddProjectWithPostfix<Projects.TransactionalOutbox_Banking_AccountService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.TransactionalOutbox_Banking_AccountService>())
            .WithReference(kafka)
            .WithReference(outboxAccountDb, Consts.DefaultDatabase)
            .WaitFor(outboxAccountDb)
            .WaitFor(kafka);

        var outboxConsumingService = builder.AddProjectWithPostfix<Projects.TransactionalOutbox_MessageConsumingService>()
            .WithReference(kafka)
            .WithEnvironment(Consts.Env_EventConsumingTopics, GetTopicName<Projects.TransactionalOutbox_Banking_AccountService>())
            .WaitFor(kafka);

        outboxConsumingService.WithParentRelationship(outboxAccountService);

        #endregion

        #region Idempotent Consumer Catalog
        var idempotentCatalogDb = postgres.AddDefaultDatabase<Projects.IdempotentConsumer_CatalogService>();
        var idempotentCatalogService = builder.AddProjectWithPostfix<Projects.IdempotentConsumer_CatalogService>()
            .WithReference(idempotentCatalogDb, Consts.DefaultDatabase)
            .WaitFor(idempotentCatalogDb);
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
